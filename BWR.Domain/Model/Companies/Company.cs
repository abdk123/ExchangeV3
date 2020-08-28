using BWR.Domain.Model.Transactions;
using BWR.ShareKernel.Common;
using System.Collections.Generic;

namespace BWR.Domain.Model.Companies
{
    public class Company: Entity
    {
        public Company()
        {
            CompanyCashes = new List<CompanyCash>();
            CompanyCashFlows = new List<CompanyCashFlow>();
            CompanyCountries = new List<CompanyCountry>();
            CompanyUsers = new List<CompanyUser>();
            Transactions = new List<Transaction>();
            
        }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        
        public virtual IList<CompanyCash> CompanyCashes { get; set; }
        public virtual IList<CompanyCashFlow> CompanyCashFlows { get; set; }
        public virtual IList<CompanyCountry> CompanyCountries { get; set; }
        public virtual IList<CompanyUser> CompanyUsers { get; set; }
        public virtual IList<Transaction> Transactions { get; set; }
        
    }
}
