using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EFCore.TimeTraveler;
using EFCore.TimeTravelerTests.DataAccess;
using FluentAssertions;
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

            await UpdateFruitStatus(appleId, FruitStatus.Rotten);

            var rottenAppleTime = DateTime.UtcNow;

            await AsTimePasses();

            await UpdateFruitStatus(appleId, FruitStatus.Fuzzy);

            var fuzzyAppleTime = DateTime.UtcNow;

            (await GetApple(appleId))
                .FruitStatus
                .Should().Be(FruitStatus.Fuzzy);

            using (new TemporalQuery(rottenAppleTime))
            {
                (await GetApple(appleId))
                    .FruitStatus
                    .Should().Be(FruitStatus.Rotten);

            }

            using (new TemporalQuery(overripeAppleTime))
            {
                (await GetApple(appleId))
                    .FruitStatus
                    .Should().Be(FruitStatus.Overripe);

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
        }

        private static async Task<Apple> GetApple(Guid appleId)
        {
            using var localScope = _serviceProvider.CreateScope();

            var context = localScope.ServiceProvider.GetService<ApplicationDbContext>();

            return await context.Apples.Where(a => a.Id == appleId).SingleAsync();
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
            await context.Database.EnsureCreatedAsync();
        }

        private static void Configure()
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            services.AddDbContext<ApplicationDbContext>();

            // builder.RegisterType<DemoService>().As<IDemoService>();
            //
            // Add other services ...
            //


            builder.Populate(services);
            var appContainer = builder.Build();
            _serviceProvider = new AutofacServiceProvider(appContainer);
        }

        private static async Task UpdateFruitStatus(Guid appleId, FruitStatus fruitStatus)
        {
            using var localScope = _serviceProvider.CreateScope();

            var context = localScope.ServiceProvider.GetService<ApplicationDbContext>();
            var myApple = await context.Apples.Where(a => a.Id == appleId).SingleAsync();

            myApple.FruitStatus = fruitStatus;

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
