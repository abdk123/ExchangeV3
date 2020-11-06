using BWR.Application.Dtos.Statement;
using System;
using System.Collections.Generic;
using BWR.Application.Dtos.ReportsDto;
using BWR.Application.Common;

namespace BWR.Application.Interfaces
{
    public interface IStatementAppService
    {
        IList<BalanceStatementDto> GetAllBalances(int coinId, DateTime to);
        ConclusionDto GetConclusion(int coinId, DateTime date);
        IList<IncomeOutcomeReport> GetPayment(int coinId, PaymentsTypeEnum paymentsTypeEnum, DateTime? form = null, DateTime? to =null, int? PaymentsTypeEntityId =null);
    }
}
