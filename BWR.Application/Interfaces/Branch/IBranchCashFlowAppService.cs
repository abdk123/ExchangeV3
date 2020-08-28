using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using BWR.Application.Dtos.Branch.BranchCashFlow;
using BWR.Application.Dtos.BranchCashFlow;

namespace BWR.Application.Interfaces.BranchCashFlow
{
    public interface IBranchCashFlowAppService 
    {
        IList<BranchCashFlowDto> GetAll();
        IList<BranchCashFlowOutputDto> Get(int? branchId, int coinId, DateTime? from, DateTime? to);
        IList<BranchCashFlowDto> Get(Expression<Func<BWR.Domain.Model.Branches.BranchCashFlow, bool>> predicate) ;

    }
}
