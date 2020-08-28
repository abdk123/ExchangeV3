using BWR.Application.Common;

namespace BWR.Application.Dtos.Setting.Country
{
    public class CountryDto: EntityDto
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        
    }
}
