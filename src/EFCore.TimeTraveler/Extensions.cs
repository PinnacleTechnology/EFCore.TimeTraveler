using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.TimeTraveler
{
    public static class Extensions
    {
        public static void EnableTemporalQuery(this EntityTypeBuilder entityTypeBuilder)
        {
            entityTypeBuilder.Metadata["EnableTemporalQuery"] = true;
        }
    }
}
