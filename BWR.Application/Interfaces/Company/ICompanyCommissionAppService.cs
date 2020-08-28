using BWR.Application.Dtos.Company.CompanyCommission;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Company
{
    public interface ICompanyCommissionAppService 
    {
        IList<CompanyCommissionsDto> GetByCompanyId(int companyId);
        bool CheckIfExist(int id,int companyCountryId, int coinId);
        CompanyCommissionsDto GetById(int id);
        CompanyCommissionDto Insert(CompanyCommissionInsertDto dto);
        CompanyCommissionDto Update(CompanyCommissionUpdateDto dto);
        CompanyCommissionUpdateDto GetForEdit(int id);
        void Delete(int id);
    }
}
