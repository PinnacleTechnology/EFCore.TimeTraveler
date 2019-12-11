using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.TimeTraveler
{
    public static class Extensions
    {
        private const string EnableTemporalQueryKey = "EnableTemporalQuery";

        /// <summary>
        /// Call entityTypeBuilder.EnableTemporalQuery() for each entity that will participate
        /// in time travel. This is generally done from an IEntityTypeConfiguration derived class
        /// or the OnModelCreating() of the DbContext.
        ///
        /// Type annotations are not currently supported.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityTypeBuilder"></param>
        /// <returns></returns>
        public static EntityTypeBuilder<TEntity> EnableTemporalQuery<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder) where TEntity : class
        {
            entityTypeBuilder.Metadata[EnableTemporalQueryKey] = true;

            // Each time an temporal entity is configured, the list of all temporal tables on the model is rebuilt
            entityTypeBuilder.Metadata.Model.EnableTemporalQuery();

            return entityTypeBuilder;
        }

        /// <summary>
        /// Call modelBuilder.EnableTemporalQuery() from your DbContext's OnModelCreating()
        /// after all entities have been configured, including any calls to
        /// entityTypeBuilder.ToTable("TableName").
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <returns>modelBuilder to allow method chaining.</returns>
        public static ModelBuilder EnableTemporalQuery(this ModelBuilder modelBuilder)
        {
            modelBuilder.Model.EnableTemporalQuery();

            return modelBuilder;
        }



        private static IMutableModel EnableTemporalQuery(this IMutableModel mutableModel, IEnumerable<string> temporalTableNames)
        {
            var temporalTables = new TemporalTables(temporalTableNames.Select(t => $"[{t}]"));

            mutableModel[TimeTravelInterceptor.TemporalTablesKey] = temporalTables;

            return mutableModel;
        }
        
        private static IMutableModel EnableTemporalQuery(this IMutableModel mutableModel)
        {
            var temporalTableNames = mutableModel.GetEntityTypes()
                .Where(type => type[EnableTemporalQueryKey] as bool? == true)
                .Select(type => type.GetTableName());

            mutableModel.EnableTemporalQuery(temporalTableNames);

            return mutableModel;
        }


    }
}
