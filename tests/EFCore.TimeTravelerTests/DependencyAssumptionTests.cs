using System;
using System.Linq;
using System.Threading.Tasks;
using EFCore.TimeTravelerTests.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace EFCore.TimeTravelerTests
{
    [TestFixture]
    internal class DependencyAssumptionTests
    {
        [Test]
        public void Check_UtcNow_Precision()
        {
            TestHelper.CurrentMachineTimerAccuracy.Should().BeLessOrEqualTo(TimeSpan.FromMilliseconds(5));
        }

        [Test]
        public async Task Respawn_ResetDb_Should_ClearAppleTemporalTable()
        {
            var context = TestHelper.GetNewDbContext();
            var appleId = Guid.Parse("00000001-8803-4263-ada6-cd12a33d8872");

            context.Apples.Add(new Apple {Id = appleId, FruitStatus = FruitStatus.Ripe});

            await context.SaveChangesAsync();

            context = TestHelper.GetNewDbContext();

            context.Apples.Count(a => a.Id == appleId).Should().Be(1);

            await TestHelper.ResetDb();

            context.Apples.Count(a => a.Id == appleId).Should().Be(0);
        }
    }
}
