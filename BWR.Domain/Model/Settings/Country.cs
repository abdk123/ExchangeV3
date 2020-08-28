using BWR.ShareKernel.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Settings
{
    public class Country: Entity
    {
        public Country()
        {
            Provinces = new List<Country>();
        }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        public int? MainCountryId { get; set; }
        [ForeignKey("MainCountryId")]
        public virtual Country MainCountry { get; set; }

        public virtual IList<Country> Provinces { get; set; }
    }
}
