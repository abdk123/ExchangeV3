using BWR.Application.Dtos.Company;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Company
{
    public interface ICompanyAppService : IGrudAppService<CompanyDto, CompanyInsertDto, CompanyUpdateDto>
    {
        IList<CompanyForDropdownDto> GetForDropdown(string name);
        bool CheckIfExist(string name, int id);
    }
}
