using EFCore.TimeTraveler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public static readonly ILoggerFactory MyLoggerFactory
            = LoggerFactory.Create(builder => { builder.AddConsole(); });

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString)
                .UseLoggerFactory(MyLoggerFactory) // Warning: Do not create a new ILoggerFactory instance each time
                .AddInterceptors(new TimeTravelInterceptor());

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var appleEntity = modelBuilder.Entity<Apple>()
                .EnableTemporalQuery();

            appleEntity.HasKey(apple => apple.Id);
            appleEntity.Property(b => b.FruitStatus)
                .IsRequired();
            
            var wormEntity = modelBuilder.Entity<Worm>()
                .EnableTemporalQuery();

            wormEntity.HasKey(worm => worm.Id);
            wormEntity.Property(worm => worm.Id)
                .ValueGeneratedOnAdd();

            wormEntity.HasMany(worm => worm.Weapons)
                .WithOne(weapon => weapon.Worm)
                .HasForeignKey(weapon => weapon.WormId);

            wormEntity.HasOne(worm => worm.Apple)
                .WithMany(apple => apple.Worms)
                .HasForeignKey(worm => worm.AppleId);

            wormEntity.Ignore(worm => worm.Friendships);
            wormEntity.Ignore(worm => worm.FriendsNames);

            var wormFriendshipEntity = modelBuilder.Entity<WormFriendship>()
                .EnableTemporalQuery();

            wormFriendshipEntity.HasKey(worm => worm.Id);

            wormFriendshipEntity.HasOne(friendship => friendship.WormA)
                .WithMany(worm => worm.FriendshipsA)
                .HasForeignKey(friendship => friendship.WormAId);

            wormFriendshipEntity.HasOne(f => f.WormB)
                .WithMany(worm => worm.FriendshipsB)
                .HasForeignKey(friendship => friendship.WormBId)
                .OnDelete(DeleteBehavior.ClientCascade);
            
            modelBuilder.Entity<WormWeapon>()
                .EnableTemporalQuery();
            
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Use the entity name instead of the Context.DbSet<T> name
                // refs https://docs.microsoft.com/en-us/ef/core/modeling/relational/tables#conventions
                modelBuilder.Entity(entityType.ClrType).ToTable(entityType.ClrType.Name);
            }
        }
    }
}
