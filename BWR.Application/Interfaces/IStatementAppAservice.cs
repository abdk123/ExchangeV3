using BWR.Application.Dtos.Statement;
using System;
using System.Collections.Generic;

namespace BWR.Application.Interfaces
{
    public interface IStatementAppService
    {
        IList<BalanceStatementDto> GetAllBalances(int coinId, DateTime? to);
        ConclusionDto GetConclusion(int coinId, DateTime date);
    }
}
