using System;
using System.Threading.Tasks;
using EFCore.TimeTraveler;
using EFCore.TimeTravelerTests.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EFCore.TimeTravelerTests
{
    [TestFixture]
    public class TemporalQueryTest
    {
        [SetUp]
        public async Task ResetDb() => await TestHelper.ResetDb();


        [Test]
        public async Task Given_SingleEntity_Should_TimeTravel()
        {
            var context = TestHelper.GetNewDbContext();
            var appleId = Guid.Parse("00000002-8803-4263-ada6-cd12a33d8872");

            context.Apples.Add(new Apple { Id = appleId, FruitStatus = FruitStatus.Ripe });
            await context.SaveChangesAsync();

            var ripeAppleTime = TestHelper.UtcNow;

            context = TestHelper.GetNewDbContext();
            var ripeApple = await context.Apples.SingleAsync(a => a.Id == appleId);
            ripeApple.FruitStatus = FruitStatus.Rotten;
            await context.SaveChangesAsync();

            context = TestHelper.GetNewDbContext();
            var currentApple = await context.Apples.AsNoTracking().SingleAsync(a => a.Id == appleId);
            currentApple.FruitStatus.Should().Be(FruitStatus.Rotten);

            using (TemporalQuery.AsOf(ripeAppleTime))
            {
                var timeTravelApple = await context.Apples.AsNoTracking().SingleAsync(a => a.Id == appleId);
                timeTravelApple.FruitStatus.Should().Be(FruitStatus.Ripe);
            }
        }
        
        [Test]
        public async Task Given_IncludedCollection_Should_TimeTravelIncludedCollection()
        {
            var context = TestHelper.GetNewDbContext();
            var appleId = Guid.Parse("00000003-8803-4263-ada6-cd12a33d8872");

            var apple = new Apple {Id = appleId, FruitStatus = FruitStatus.Rotten};
            apple.AddWorm("Gav");
            context.Apples.Add(apple);

            await context.SaveChangesAsync();

            var rottenAppleTime = TestHelper.UtcNow;

            context = TestHelper.GetNewDbContext();

            apple = await context.Apples.SingleAsync(a => a.Id == appleId);
            apple.AddWorm("G-Dog");
            apple.AddWorm("Gavin' A Laugh");

            await context.SaveChangesAsync();

            context = TestHelper.GetNewDbContext();

            var currentApple = await context.Apples
                .Include(a => a.Worms)
                .SingleAsync(a => a.Id == appleId);

            currentApple.Worms.Count.Should().Be(3);

            using (TemporalQuery.AsOf(rottenAppleTime))
            {
                // Let's use the same context for time travel
                var timeTravelApple = await context.Apples
                    .Include(a => a.Worms)
                    // All temporal queries must be .AsNoTracking()
                    .AsNoTracking()
                    .SingleAsync(a => a.Id == appleId);

                timeTravelApple.Worms.Count.Should().Be(1);
            }
        }


    }
}
