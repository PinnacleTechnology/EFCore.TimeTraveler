using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EFCore.TimeTravelerTests.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Respawn;

namespace EFCore.TimeTravelerTests
{
    [SetUpFixture]
    public class DatabaseSetupFixture
    {
        //TODO: Hardcoded = bad
        public static readonly string ConnectionString =
            @"Server=localhost\SQLEXPRESS;Database=EFCoreTimeTravelerTests;Trusted_Connection=True;ConnectRetryCount=0";

        public static AutofacServiceProvider ServiceProvider
        {
            get;
            private set;
        }

        public static async Task ResetDb()
        {
            await _checkpoint.Reset(ConnectionString);
        }

        private static Checkpoint _checkpoint;
        
        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            Configure();
            await ScaffoldDb();
        }

        private static void Configure()
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            services.AddDbContext<ApplicationDbContext>();

            builder.Populate(services);
            var appContainer = builder.Build();
            ServiceProvider = new AutofacServiceProvider(appContainer);
        }

        private static async Task ScaffoldDb()
        {
            var context = ServiceProvider.GetService<ApplicationDbContext>();

            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();

            _checkpoint = new Checkpoint
            {
                TablesToIgnore = new[] {"__EFMigrationsHistory"},
                CheckTemporalTables = true
            };
        }


        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
        }
    }
}
