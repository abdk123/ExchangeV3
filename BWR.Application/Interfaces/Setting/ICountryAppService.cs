using BWR.Application.Dtos.Setting.Country;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Setting
{
    public interface ICountryAppService : IGrudAppService<CountryDto, CountryInsertDto, CountryUpdateDto>
    {
        IList<CountryForDropdownDto> GetForDropdown(string name);
        IList<CountryForDropdownDto> GetCountriesAndProvinces();
        bool CheckIfExist(string name, int id);

    }
}
