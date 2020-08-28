using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Logs;
using BWR.Domain.Model.Security;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using BWR.Domain.Model.Treasures;
using BWR.Infrastructure.Configuration.Common;
using BWR.Infrastructure.Configuration.Security;
using BWR.Infrastructure.Configuration.Treasuries;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace BWR.Infrastructure.Context
{
    public class MainContext : DbContext
    {
        public MainContext() : base("DefaultConnection")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Configurations.Add(new UserConfiguration());
            modelBuilder.Configurations.Add(new RoleConfiguration());
            modelBuilder.Configurations.Add(new ExternalLoginConfiguration());
            modelBuilder.Configurations.Add(new ClaimConfiguration());
            modelBuilder.Configurations.Add(new TreasuryConfiguration());
            modelBuilder.Configurations.Add(new ExchangeConfiguration());
            
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity.GetType().GetProperty("Modified") != null || entry.Entity.GetType().GetProperty("Created") != null))
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("Created").CurrentValue = DateTime.Now;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property("Modified").CurrentValue = DateTime.Now;
                }
            }
            return base.SaveChanges();
        }

        public DbSet<TreasuryCash> TreasuryCashs { get; set; }
        public DbSet<TreasuryMoneyAction> TreasuryMoneyActions { get; set; }
        public DbSet<Treasury> Treasurys { get; set; }
        public DbSet<UserTreasuery> UserTreasueries { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<BWR.Domain.Model.Settings.Action> Actions { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Coin> Coins { get; set; }
        public DbSet<Country> Countrys { get; set; }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<ExternalLogin> ExternalLogins { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Company> Companys { get; set; }
        public DbSet<CompanyCash> CompanyCashs { get; set; }
        public DbSet<CompanyCashFlow> CompanyCashFlows { get; set; }
        public DbSet<CompanyCommission> CompanyCommissions { get; set; }
        public DbSet<CompanyCountry> CompanyCountrys { get; set; }
        public DbSet<CompanyUser> CompanyUsers { get; set; }
        public DbSet<BoxAction> BoxActions { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<MoneyAction> MoenyActions { get; set; }
        public DbSet<PublicExpense> PublicExpenses { get; set; }
        public DbSet<PublicIncome> PublicIncomes { get; set; }
        public DbSet<PublicMoney> PublicMoneys { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientAttatchment> ClientAttatchments { get; set; }
        public DbSet<ClientCash> ClientCashs { get; set; }
        public DbSet<ClientCashFlow> ClientCashFlows { get; set; }
        public DbSet<ClientPhone> ClientPhones { get; set; }
        //public DbSet<ClientType> ClientTypes { get; set; }
        public DbSet<Branch> Branchs { get; set; }
        public DbSet<BranchCash> BranchCashs { get; set; }
        public DbSet<BranchCashFlow> BranchCashFlows { get; set; }
        public DbSet<BranchCommission> BranchCommissions { get; set; }

    }
}
