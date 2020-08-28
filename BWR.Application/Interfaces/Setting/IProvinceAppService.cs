using BWR.Application.Dtos.Setting.Provinces;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Setting
{
    public interface IProvinceAppService : IGrudAppService<ProvinceDto, ProvinceInsertDto, ProvinceUpdateDto>
    {
        IList<ProvinceForDropdownDto> GetForDropdown(string name);
        bool CheckIfExist(string name, int countryId, int id);

        IList<ProvinceDto> GetProvinceForSpecificCountry(int countryId);

        //ProvinceDto AddProvinceForSpecificCountry(CountryDto province, int countryId);
        //ProvinceDto UpdateProvinceForSpecificCountry(CountryDto province, int countryId);
        //ProvinceDto DeleteProvinceForSpecificCountry(CountryDto province, int countryId);
    }
}
