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
        DataTablesDto GetPayment(int draw,int start,int length,int coinId, PaymentsTypeEnum paymentsTypeEnum, DateTime? form = null, DateTime? to =null, int? PaymentsTypeEntityId =null);
        DataTablesDto GetIncme(int draw, int start, int length, int coinId, PaymentsTypeEnum paymentsTypeEnum, DateTime? from, DateTime? to, int? incomeFromEntitiyId);
        IList<ClearigStatement> GetClearing(int coinId, IncomeOrOutCame incomeOrOutCame, DateTime? from, DateTime? to, ClearingAccountType fromAccountType, int?fromAccountId, ClearingAccountType toAccountType, int? toAccountId);


    }
}
