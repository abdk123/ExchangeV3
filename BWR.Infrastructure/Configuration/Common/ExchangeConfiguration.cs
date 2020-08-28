using BWR.Domain.Model.Common;
using System.Data.Entity.ModelConfiguration;

namespace BWR.Infrastructure.Configuration.Common
{
    public class ExchangeConfiguration: EntityTypeConfiguration<Exchange>
    {
        public ExchangeConfiguration()
        {
            HasRequired(x => x.Branch)
               .WithMany()
               .HasForeignKey(x => x.BranchId);

            HasRequired(x => x.FirstCoin)
               .WithMany()
               .HasForeignKey(x => x.FirstCoinId)
               .WillCascadeOnDelete(false);

            HasRequired(x => x.SecoundCoin)
               .WithMany()
               .HasForeignKey(x => x.SecoundCoinId)
               .WillCascadeOnDelete(false);

            HasRequired(x => x.MainCoin)
               .WithMany()
               .HasForeignKey(x => x.MainCoinId)
               .WillCascadeOnDelete(false);

        }
    }
}
