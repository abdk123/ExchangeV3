using BWR.Application.Common;
using BWR.Application.Dtos.Company;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Company
{
    public interface ICompanyCountryAppService
    {
        CompanyCountryDto Insert(CompanyCountryDto dto);
        CompanyCountryDto Update(CompanyCountryDto dto);
        IList<DtoForDropdown> GetCompanyCountriesForDropdown(int companyId);
        IList<CompanyCountriesDto> GetCountriesForCompany(int companyId);
        bool CheckIfExist(int companyId,int countryId);
        void Delete(int id);

    }
}
