using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EFCore.TimeTraveler;
using EFCore.TimeTravelerTests.DataAccess;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.TimeTravelerTests
{
    class Program
    {
        private static AutofacServiceProvider _serviceProvider;

        static async Task Main(string[] args)
        {
            Configure();

            await ScaffoldDb();

            var appleId = Guid.NewGuid();

            await SetInitialFruitStatus(appleId);

            var unripeAppleTime = DateTime.UtcNow;

            await AsTimePasses();

            await UpdateFruitStatus(appleId, FruitStatus.Ripe);

            var ripeAppleTime = DateTime.UtcNow;

            await AsTimePasses();

            await UpdateFruitStatus(appleId, FruitStatus.Overripe);

            var overripeAppleTime = DateTime.UtcNow;

            await AsTimePasses();

            await UpdateFruitStatus(appleId, FruitStatus.Rotten, new[]{"Moe"});

            var rottenAppleTime = DateTime.UtcNow;

            await AsTimePasses();

            await UpdateFruitStatus(appleId, FruitStatus.Fuzzy, new[] { "Hairy", "Curly" });

            var fuzzyAppleTime = DateTime.UtcNow;

            using var assertionScope = new AssertionScope();
            ;

            (await GetApple(appleId))
                .Should()
                .BeEquivalentTo(new
                {
                    FruitStatus = FruitStatus.Fuzzy,
                    Worms = new[] { new {Name = "Hairy"}, new {Name = "Curly"}, new {Name = "Moe"} }
                }, options => options.ExcludingMissingMembers());

            using (new TemporalQuery(rottenAppleTime))
            {
                (await GetApple(appleId))
                    .Should()
                    .BeEquivalentTo(new
                    {
                        FruitStatus = FruitStatus.Rotten,
                        Worms = new[] { new { Name = "Moe" } }
                    }, options => options.ExcludingMissingMembers());

            }

            using (new TemporalQuery(overripeAppleTime))
            {
                (await GetApple(appleId))
                    .Should()
                    .BeEquivalentTo(new
                    {
                        FruitStatus = FruitStatus.Overripe,
                        Worms = Array.Empty<Worm>()
                    }, options => options.ExcludingMissingMembers());
            }

            using (new TemporalQuery(ripeAppleTime))
            {
                (await GetApple(appleId))
                    .FruitStatus
                    .Should().Be(FruitStatus.Ripe);

            }

            using (new TemporalQuery(unripeAppleTime))
            {
                (await GetApple(appleId))
                    .FruitStatus
                    .Should().Be(FruitStatus.Unripe);

            }


            using (new TemporalQuery(rottenAppleTime))
            {
                using var localScope = _serviceProvider.CreateScope();
                var context = localScope.ServiceProvider.GetService<ApplicationDbContext>();
                var rottenAppleWorms = await context.Apples
                    .Where(a => a.Id == appleId)
                    .Select(a => a.Worms)
                    .AsNoTracking().ToListAsync();

                rottenAppleWorms
                    .Should()
                    .BeEquivalentTo(new[] { new { Name = "Moe" } },
                        options => options.ExcludingMissingMembers());
            }
        }

        private static async Task<Apple> GetApple(Guid appleId)
        {
            using var localScope = _serviceProvider.CreateScope();

            var context = localScope.ServiceProvider.GetService<ApplicationDbContext>();

            return await context.Apples.Include(a => a.Worms).Where(a => a.Id == appleId).AsNoTracking().SingleAsync();
        }


        private static async Task AsTimePasses()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(150));
        }

        private static async Task ScaffoldDb()
        {
            using var localScope = _serviceProvider.CreateScope();

            var context = localScope.ServiceProvider.GetService<ApplicationDbContext>();

            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }

        private static void Configure()
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            services.AddDbContext<ApplicationDbContext>();

            builder.Populate(services);
            var appContainer = builder.Build();
            _serviceProvider = new AutofacServiceProvider(appContainer);
        }

        private static async Task UpdateFruitStatus(Guid appleId, FruitStatus fruitStatus, string[] worms = null)
        {
            using var localScope = _serviceProvider.CreateScope();
            worms ??= Array.Empty<string>();
            var context = localScope.ServiceProvider.GetService<ApplicationDbContext>();
            var myApple = await context.Apples.Where(a => a.Id == appleId).SingleAsync();

            myApple.FruitStatus = fruitStatus;
            myApple.Worms.AddRange(worms.Select(wormName => new Worm{Name = wormName}));

            await context.SaveChangesAsync();
        }

        private static async Task SetInitialFruitStatus(Guid appleId)
        {
            using var localScope = _serviceProvider.CreateScope();

            var context = localScope.ServiceProvider.GetService<ApplicationDbContext>();

            var myApple = new Apple {Id = appleId, FruitStatus = FruitStatus.Unripe};
            context.Apples.Add(myApple);

            await context.SaveChangesAsync();
        }
    }
}
