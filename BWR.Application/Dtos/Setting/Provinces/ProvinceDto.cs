using BWR.Application.Common;
using BWR.Application.Dtos.Setting.Country;

namespace BWR.Application.Dtos.Setting.Provinces
{
    public class ProvinceDto : EntityDto
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public CountryDto MainCountry { get; set; }

    }
}
