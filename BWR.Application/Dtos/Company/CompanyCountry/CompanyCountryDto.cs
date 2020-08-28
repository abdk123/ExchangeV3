
using BWR.Application.Common;
using BWR.Application.Dtos.Setting.Country;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Company
{
    public class CompanyCountryDto : EntityDto
    {
        public int CompanyId { get; set; }

        public int CountryId { get; set; }
        
    }
}
