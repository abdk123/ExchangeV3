

using BWR.Application.Dtos.Company;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BWR.Application.Interfaces.Company
{
    public interface ICompanyCashAppService
    {
        IList<CompanyCashesDto> GetAll();
        IList<CompanyCashesDto> GetCompanyCashes(int companyId);
        CompanyCashDto Insert(CompanyCashDto dto);
        CompanyCashDto Update(CompanyCashDto dto);
        CompanyCashDto UpdateBalance(CompanyCashesDto dto);
        Task<CompanyCashDto> InsertAsync(CompanyCashDto companyCashInsertDto);
    }
}
