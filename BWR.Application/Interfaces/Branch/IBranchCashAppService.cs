using System.Collections.Generic;
using System.Threading.Tasks;
using BWR.Application.Dtos.Branch;
using BWR.Domain.Model.Branches;

namespace BWR.Application.Interfaces.Branch
{
    public interface IBranchCashAppService
    {
        IList<BranchCashDto> GetAll();
        IList<BranchCashDto> GetForSpecificBranch(int branchId);
        BranchCashUpdateDto GetForEdit(int id);
        BranchCashDto Insert(BranchCashInsertDto dto);
        Task<BranchCashDto> InsertAsync(BranchCashInsertDto dto);
        void UpdateAll(IList<BranchCashUpdateDto> branchCashes);
        BranchCashDto Update(BranchCashUpdateDto branchCash);
        dynamic GetActualBalance(int coinId, int branchId);
        
    }
}
