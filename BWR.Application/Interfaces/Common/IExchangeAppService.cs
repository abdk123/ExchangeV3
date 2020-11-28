

using BWR.Application.Dtos.Common;

namespace BWR.Application.Interfaces.Common
{
    public interface IExchangeAppService
    {
        bool ExchangeForBranch(ExchangeInputDto input);
        bool ExchangeForClient(ExchangeInputDto input);
        bool ExchangeForCompany(ExchangeInputDto input);
        decimal CalcForFirstCoin(int sellingCoinId, int purchasingCoinId, decimal amountFromFirstCoin);
    }
}
