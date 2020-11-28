using System;
using System.Collections.Generic;
using BWR.Application.Dtos.Client.ClientCashFlow;
using BWR.Application.Dtos.Statement;

namespace BWR.Application.Interfaces.Client
{
    public interface IClientCashFlowAppService
    {
        IList<ClientCashFlowOutputDto> Get(ClientCashFlowInputDto input);
        IList<BalanceStatementDto> GetForStatement(int coinId, DateTime to);
        ClientMatchDto ConvertMatchingStatus(ClientMatchDto dto);
        IList<ClientBalanceDto> GetBalanceForClient(int clientId, int coinId);
    }
}
