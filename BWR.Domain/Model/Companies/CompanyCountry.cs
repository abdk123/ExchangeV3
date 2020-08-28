using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Companies
{
    public class CompanyCountry: Entity
    {
        public CompanyCountry()
        {
            this.CompanyComissions = new List<CompanyCommission>();
            this.CompanyCountries = new List<CompanyCountry>();
        }

        public bool IsEnabled { get; set; }

        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        public int CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }

        public virtual IList<CompanyCommission> CompanyComissions { get; set; }
        public virtual IList<CompanyCountry> CompanyCountries { get; set; }
    }
}