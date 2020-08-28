
using BWR.Domain.Model.Security;
using System.Data.Entity.ModelConfiguration;

namespace BWR.Infrastructure.Configuration.Security
{
    public class RoleConfiguration : EntityTypeConfiguration<Role>
    {
        public RoleConfiguration()
        {
            ToTable("Role");

            HasKey(x => x.RoleId)
                .Property(x => x.RoleId)
                .HasColumnName("RoleId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            Property(x => x.Name)
                .HasColumnName("Name")
                .HasColumnType("nvarchar")
                .HasMaxLength(256)
                .IsRequired();

            HasMany(x => x.Users)
                .WithMany(x => x.Roles)
                .Map(x =>
                {
                    x.ToTable("UserRole");
                    x.MapLeftKey("RoleId");
                    x.MapRightKey("UserId");
                });
        }
    }
}