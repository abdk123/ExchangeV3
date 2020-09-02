using BWR.Application.Dtos.Common;
using BWR.Application.Dtos.Company;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using companyModel= BWR.Domain.Model.Companies;
namespace BWR.Application.Interfaces.Company
{
    public interface ICompanyAppService : IGrudAppService<CompanyDto, CompanyInsertDto, CompanyUpdateDto>
    {
        IList<CompanyForDropdownDto> GetForDropdown(string name);
        bool CheckIfExist(string name, int id);
        IList<Select2Dto<int>> GetSelect2(Expression<Func<companyModel.Company, bool>> predicate = null);

    }
}
