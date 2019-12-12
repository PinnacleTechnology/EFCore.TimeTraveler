using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using EFCore.TimeTraveler;
using EFCore.TimeTravelerTests.DataAccess;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace EFCore.TimeTravelerTests
{
    [TestFixture]
    public class TemporalQueryTest
    {
        private static AutofacServiceProvider ServiceProvider => DatabaseSetupFixture.ServiceProvider;

        [SetUp]
        public async Task ResetDb()
        {
            await DatabaseSetupFixture.ResetDb();
        }




        [Test]
        public async Task Respawn_ResetDb_Should_ClearAppleTemporalTable()
        {
            var context = ServiceProvider.GetService<ApplicationDbContext>();
            var appleId = Guid.Parse("f7a08c85-8803-4263-ada6-cd12a33d8872");

            context.Apples.Add(new Apple {Id = appleId, FruitStatus = FruitStatus.Ripe});

            await context.SaveChangesAsync();

            context = ServiceProvider.GetService<ApplicationDbContext>();

            context.Apples.Count(a => a.Id == appleId).Should().Be(1);

            await ResetDb();

            context.Apples.Count(a => a.Id == appleId).Should().Be(0);
        }

        [Test]
        public async Task Given_SingleEntity_Should_TimeTravel()
        {
            var context = GetNewDbContext();
            var appleId = Guid.Parse("f7a08c85-8803-4263-ada6-cd12a33d8872");

            context.Apples.Add(new Apple { Id = appleId, FruitStatus = FruitStatus.Ripe });
            await context.SaveChangesAsync();

            var ripeAppleTime = DateTime.UtcNow;

            context = GetNewDbContext();
            var ripeApple = await context.Apples.SingleAsync(a => a.Id == appleId);
            ripeApple.FruitStatus = FruitStatus.Rotten;
            await context.SaveChangesAsync();

            context = GetNewDbContext();
            var currentApple = await context.Apples.AsNoTracking().SingleAsync(a => a.Id == appleId);
            currentApple.FruitStatus.Should().Be(FruitStatus.Rotten);

            using (TemporalQuery.At(ripeAppleTime))
            {
                var timeTravelApple = await context.Apples.AsNoTracking().SingleAsync(a => a.Id == appleId);
                timeTravelApple.FruitStatus.Should().Be(FruitStatus.Ripe);
            }
        }

        private static ApplicationDbContext GetNewDbContext()
        {
            return ServiceProvider.GetService<ApplicationDbContext>();
        }
    }
}
