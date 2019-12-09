using EFCore.TimeTraveler;
using Microsoft.EntityFrameworkCore;

namespace EFCore.TimeTravelerTests.DataAccess
{
    class ApplicationDbContext : DbContext
    {
        //TODO: Hardcoded = bad
        private const string ConnectionString = @"Server=localhost\SQLEXPRESS;Database=EFCoreTimeTravelerTests;Trusted_Connection=True;ConnectRetryCount=0";


        public ApplicationDbContext()
        {
            
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> contextOptions)
        :base(contextOptions)
        {
            
        }

        public DbSet<Apple> Apples { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString)
                .AddInterceptors(new TimeTravelInterceptor());

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var appleEntity = modelBuilder.Entity<Apple>();
            appleEntity.HasKey(apple => apple.Id);
            appleEntity.Property(b => b.FruitStatus)
                .IsRequired();
        }
    }
}
