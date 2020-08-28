using BWR.Domain.Model.Treasures;
using System.Data.Entity.ModelConfiguration;

namespace BWR.Infrastructure.Configuration.Treasuries
{
    public class TreasuryConfiguration : EntityTypeConfiguration<Treasury>
    {
        public TreasuryConfiguration()
        {
            HasRequired(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .WillCascadeOnDelete(false);
        }
    }
}
