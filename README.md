# EFCore.TimeTraveler
v0.26 - Alpha - Prototype

Allow full-featured Entity Framework Core queries against SQL Server [Temporal Tables](https://docs.microsoft.com/en-us/sql/relational-databases/tables/temporal-tables?view=sql-server-ver15).

## Background
EF Core [does not natively support](https://github.com/aspnet/EntityFrameworkCore/issues/4693) temporal tables.  You may query a single temporal table using [`.FromSqlRaw(...)`](https://docs.microsoft.com/en-us/ef/core/querying/raw-sql) or `.FromSqlInterpolated(...)`.  Multiple temporal tables can be queried using the same raw SQL functionality with LINQ Join.  Additionally, the [EfCoreTemporalTable](https://www.nuget.org/packages/EfCoreTemporalTable/) library provides a nice syntax for this functionality.

However, any related data from `Include(...)` or navigation properties is not able to be queried from temporal history with EF Core.  In my current project, we are using these features of EF Core to write clear and concise LINQ queries and projections of object graphs for models.  We now have a requirement

## Prerequisites
* EF Core 3.1 (Supports .NETStandard 2.0)
* SQL Server 2016 or higher or Azure SQL (For Temporal Table Support)

## Assumptions
* EF ALWAYS generates SQL with table names surrounded by square brackets.
* EF ALWAYS uses a table alias in generated SQL, so that the table name does not get repeated in the SQL unless joining to the same table.

## Example Usage
### EF Core Mapping
```csharp
    var appleEntity = modelBuilder.Entity<Apple>()
        .EnableTemporalQuery();

    appleEntity.HasKey(apple => apple.Id);

    var wormEntity = modelBuilder.Entity<Worm>()
        .EnableTemporalQuery();

    wormEntity.HasOne(worm => worm.Apple)
        .WithMany(apple => apple.Worms)
        .HasForeignKey(worm => worm.AppleId);        
```

### EF Core LINQ Query
```csharp
    var appleCurrentState = await context.Apples
        .Include(apple => apple.Worms)
        .Where(a => a.Id == appleId)
        .AsNoTracking()
        .SingleAsync();   

    appleCurrentState.Worms.Count().Should().Be(3);

    using (TemporalQuery.At(ripeAppleTime))
    {
        var applePriorState = await context.Apples
            .Include(apple => apple.Worms)
            .Where(a => a.Id == appleId)
            .AsNoTracking()
            .SingleAsync();                     
    
        applePriorState.Worms.Count().Should().Be(0);
    }
```

### More Complicated Example
See [/tests/EFCore.TimeTravelerTests/EndToEndTest.cs](/tests/EFCore.TimeTravelerTests/EndToEndTest.cs)
#### Complicated LINQ Query
```csharp
    return await context.Apples
        .Include(apple => apple.Worms)
        .ThenInclude(worm => worm.Weapons)
        .Include(apple => apple.Worms)
        .ThenInclude(worm => worm.FriendshipsA)
        .ThenInclude(friendship => friendship.WormB)
        .ThenInclude(worm => worm.Weapons)
        .Include(apple => apple.Worms)
        .ThenInclude(worm => worm.FriendshipsB)
        .ThenInclude(friendship => friendship.WormA)
        .ThenInclude(worm => worm.Weapons)
        .Where(a => a.Id == appleId)
        .AsNoTracking()
        .SingleAsync();
```

#### SQL Produced By EF Core 3.1
```sql
      SELECT [t].[Id], [t].[FruitStatus], [t2].[Id], [t2].[AppleId], [t2].[Name], [t2].[Id0], [t2].[Name0], [t2].[WormId], [t2].[Id1], [t2].[WormAId], [t2].[WormBId], [t2].[Id00], [t2].[AppleId0], [t2].[Name1], [t2].[Id10], [t2].[Name00], [t2].[WormId0], [t2].[Id2], [t2].[WormAId0], [t2].[WormBId0], [t2].[Id01], [t2].[AppleId1], [t2].[Name2], [t2].[Id11], [t2].[Name01], [t2].[WormId1]
      FROM (
          SELECT TOP(2) [a].[Id], [a].[FruitStatus]
          FROM [Apple] AS [a]
          WHERE [a].[Id] = @__appleId_0
      ) AS [t]
      LEFT JOIN (
          SELECT [w].[Id], [w].[AppleId], [w].[Name], [w0].[Id] AS [Id0], [w0].[Name] AS [Name0], [w0].[WormId], [t0].[Id] AS [Id1], [t0].[WormAId], [t0].[WormBId], [t0].[Id0] AS [Id00], [t0].[AppleId] AS [AppleId0], [t0].[Name] AS [Name1], [t0].[Id1] AS [Id10], [t0].[Name0] AS [Name00], [t0].[WormId] AS [WormId0], [t1].[Id] AS [Id2], [t1].[WormAId] AS [WormAId0], [t1].[WormBId] AS [WormBId0], [t1].[Id0] AS [Id01], [t1].[AppleId] AS [AppleId1], [t1].[Name] AS [Name2], [t1].[Id1] AS [Id11], [t1].[Name0] AS [Name01], [t1].[WormId] AS [WormId1]
          FROM [Worm] AS [w]
          LEFT JOIN [WormWeapon] AS [w0] ON [w].[Id] = [w0].[WormId]
          LEFT JOIN (
              SELECT [w1].[Id], [w1].[WormAId], [w1].[WormBId], [w2].[Id] AS [Id0], [w2].[AppleId], [w2].[Name], [w3].[Id] AS [Id1], [w3].[Name] AS [Name0], [w3].[WormId]
              FROM [WormFriendship] AS [w1]
              INNER JOIN [Worm] AS [w2] ON [w1].[WormBId] = [w2].[Id]
              LEFT JOIN [WormWeapon] AS [w3] ON [w2].[Id] = [w3].[WormId]
          ) AS [t0] ON [w].[Id] = [t0].[WormAId]
          LEFT JOIN (
              SELECT [w4].[Id], [w4].[WormAId], [w4].[WormBId], [w5].[Id] AS [Id0], [w5].[AppleId], [w5].[Name], [w6].[Id] AS [Id1], [w6].[Name] AS [Name0], [w6].[WormId]
              FROM [WormFriendship] AS [w4]
              INNER JOIN [Worm] AS [w5] ON [w4].[WormAId] = [w5].[Id]
              LEFT JOIN [WormWeapon] AS [w6] ON [w5].[Id] = [w6].[WormId]
          ) AS [t1] ON [w].[Id] = [t1].[WormBId]
      ) AS [t2] ON [t].[Id] = [t2].[AppleId]
      ORDER BY [t].[Id], [t2].[Id], [t2].[Id0], [t2].[Id1], [t2].[Id00], [t2].[Id10], [t2].[Id2], [t2].[Id01], [t2].[Id11]
```
### SQL Produced By EF Core 3.1 With EFCore.TimeTraveler
```sql
      SELECT [t].[Id], [t].[FruitStatus], [t2].[Id], [t2].[AppleId], [t2].[Name], [t2].[Id0], [t2].[Name0], [t2].[WormId], [t2].[Id1], [t2].[WormAId], [t2].[WormBId], [t2].[Id00], [t2].[AppleId0], [t2].[Name1], [t2].[Id10], [t2].[Name00], [t2].[WormId0], [t2].[Id2], [t2].[WormAId0], [t2].[WormBId0], [t2].[Id01], [t2].[AppleId1], [t2].[Name2], [t2].[Id11], [t2].[Name01], [t2].[WormId1]
      FROM (
          SELECT TOP(2) [a].[Id], [a].[FruitStatus]
          FROM [Apple] FOR SYSTEM_TIME AS OF @TimeTravelDate AS [a]
          WHERE [a].[Id] = @__appleId_0
      ) AS [t]
      LEFT JOIN (
          SELECT [w].[Id], [w].[AppleId], [w].[Name], [w0].[Id] AS [Id0], [w0].[Name] AS [Name0], [w0].[WormId], [t0].[Id] AS [Id1], [t0].[WormAId], [t0].[WormBId], [t0].[Id0] AS [Id00], [t0].[AppleId] AS [AppleId0], [t0].[Name] AS [Name1], [t0].[Id1] AS [Id10], [t0].[Name0] AS [Name00], [t0].[WormId] AS [WormId0], [t1].[Id] AS [Id2], [t1].[WormAId] AS [WormAId0], [t1].[WormBId] AS [WormBId0], [t1].[Id0] AS [Id01], [t1].[AppleId] AS [AppleId1], [t1].[Name] AS [Name2], [t1].[Id1] AS [Id11], [t1].[Name0] AS [Name01], [t1].[WormId] AS [WormId1]
          FROM [Worm] FOR SYSTEM_TIME AS OF @TimeTravelDate AS [w]
          LEFT JOIN [WormWeapon] FOR SYSTEM_TIME AS OF @TimeTravelDate AS [w0] ON [w].[Id] = [w0].[WormId]
          LEFT JOIN (
              SELECT [w1].[Id], [w1].[WormAId], [w1].[WormBId], [w2].[Id] AS [Id0], [w2].[AppleId], [w2].[Name], [w3].[Id] AS [Id1], [w3].[Name] AS [Name0], [w3].[WormId]
              FROM [WormFriendship] FOR SYSTEM_TIME AS OF @TimeTravelDate AS [w1]
              INNER JOIN [Worm] FOR SYSTEM_TIME AS OF @TimeTravelDate AS [w2] ON [w1].[WormBId] = [w2].[Id]
              LEFT JOIN [WormWeapon] FOR SYSTEM_TIME AS OF @TimeTravelDate AS [w3] ON [w2].[Id] = [w3].[WormId]
          ) AS [t0] ON [w].[Id] = [t0].[WormAId]
          LEFT JOIN (
              SELECT [w4].[Id], [w4].[WormAId], [w4].[WormBId], [w5].[Id] AS [Id0], [w5].[AppleId], [w5].[Name], [w6].[Id] AS [Id1], [w6].[Name] AS [Name0], [w6].[WormId]
              FROM [WormFriendship] FOR SYSTEM_TIME AS OF @TimeTravelDate AS [w4]
              INNER JOIN [Worm] FOR SYSTEM_TIME AS OF @TimeTravelDate AS [w5] ON [w4].[WormAId] = [w5].[Id]
              LEFT JOIN [WormWeapon] FOR SYSTEM_TIME AS OF @TimeTravelDate AS [w6] ON [w5].[Id] = [w6].[WormId]
          ) AS [t1] ON [w].[Id] = [t1].[WormBId]
      ) AS [t2] ON [t].[Id] = [t2].[AppleId]
      ORDER BY [t].[Id], [t2].[Id], [t2].[Id0], [t2].[Id1], [t2].[Id00], [t2].[Id10], [t2].[Id2], [t2].[Id01], [t2].[Id11]
```

