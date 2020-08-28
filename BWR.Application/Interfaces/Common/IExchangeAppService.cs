

namespace BWR.Application.Interfaces.Common
{
    public interface IExchangeAppService
    {
        bool ExchangeForBranch(int sellingCoinId, int purchasingCoinId, decimal firstAmount);
        bool ExchangeForClient(int clientId, int sellingCoinId, int purchasingCoinId, decimal firstAmount);
        bool ExchangeForCompany(int clientId, int sellingCoinId, int purchasingCoinId, decimal firstAmount);
        decimal CalcForFirstCoin(int sellingCoinId, int purchasingCoinId, decimal amountFromFirstCoin);
    }
}
