# EFCore.TimeTraveler
v0.02 - Prototype/Experiment

Allow full-featured Entity Framwork Core queries against SQL Server [Temporal Tables](https://docs.microsoft.com/en-us/sql/relational-databases/tables/temporal-tables?view=sql-server-ver15).

## Background
EF Core does not natively support temporal tables.  You may query a single temporal table using [`.FromSqlRaw(...)`](https://docs.microsoft.com/en-us/ef/core/querying/raw-sql) or `.FromSqlInterpolated(...)`.  Multiple temporal tables can be queried using the same raw SQL functionality with LINQ Join.  Additionally, the [EfCoreTemporalTable](https://www.nuget.org/packages/EfCoreTemporalTable/) library provides a nice syntax for this functionality.

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
See [/tests/EFCore.TimeTravelerTests/Program.cs](/tests/EFCore.TimeTravelerTests/Program.cs)