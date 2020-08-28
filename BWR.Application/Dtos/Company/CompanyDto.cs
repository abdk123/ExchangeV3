using BWR.Application.Common;
using System.Collections.Generic;

namespace BWR.Application.Dtos.Company
{
    public class CompanyDto:EntityDto
    {
        
        public CompanyDto()
        {
            CompanyCountries = new List<CompanyCountryDto>();
            CompanyCashes = new List<CompanyCashDto>();
        }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        public IList<CompanyCountryDto> CompanyCountries { get; set; }
        public IList<CompanyCashDto> CompanyCashes { get; set; }
    }
}
