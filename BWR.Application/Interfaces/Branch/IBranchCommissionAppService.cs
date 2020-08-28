using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Branch.BranchCommission;

namespace BWR.Application.Interfaces.Branch
{
    public interface IBranchCommissionAppService : IGrudAppService<BranchCommissionDto, BranchCommissionInsertDto, BranchCommissionUpdateDto>
    {
        decimal CalcComission(BranchCommissionInputDto input);
    }
}
