using System;
using System.Collections.Generic;
using BWR.Application.Dtos.Company.CompanyCashFlow;
using BWR.Application.Dtos.Statement;

namespace BWR.Application.Interfaces.Company
{
    public interface ICompanyCashFlowAppService 
    {
        IList<CompanyCashFlowOutputDto> Get(CompanyCashFlowInputDto input);
        IList<BalanceStatementDto> GetForStatement(int coinId, DateTime to);
        CompanyMatchDto ConvertMatchingStatus(CompanyMatchDto dto);
        IList<CompanyBalanceDto> GetBalanceForCompany(int companyId, int coinId);
        void Shaded(int id, bool value);
    }
}
