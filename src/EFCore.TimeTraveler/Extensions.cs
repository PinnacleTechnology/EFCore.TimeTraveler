using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.TimeTraveler
{
    public static class Extensions
    {
        public static EntityTypeBuilder<TEntity> EnableTemporalQuery<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder) where TEntity : class
        {
            entityTypeBuilder.Metadata["EnableTemporalQuery"] = true;

            return entityTypeBuilder;
        }
    }
}
