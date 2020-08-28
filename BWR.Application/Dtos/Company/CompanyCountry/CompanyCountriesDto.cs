using BWR.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Dtos.Company
{
    public class CompanyCountriesDto:EntityDto
    {
        public int CompanyId { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public bool IsEnabled { get; set; }
    }
}
