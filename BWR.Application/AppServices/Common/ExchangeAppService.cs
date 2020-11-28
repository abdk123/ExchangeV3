using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Common;
using BWR.Application.Interfaces.Common;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Treasures;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.AppServices.Common
{
    public class ExchangeAppService: IExchangeAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public ExchangeAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public bool ExchangeForBranch(ExchangeInputDto input)
        {
            try
            {
                int treasuryId = _appSession.GetCurrentTreasuryId();
                var firstAmount = input.ActionType == 1 ? -input.AmountOfFirstCoin : input.AmountOfFirstCoin;
                var secondAmount = input.ActionType == 1 ? input.AmoutOfSecondCoin : -input.AmoutOfSecondCoin;

                var mainCoin = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.IsMainCoin == true).FirstOrDefault();
                var firstCoinExchange = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == input.FirstCoinId).FirstOrDefault();
                var secoundCoinExchange = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == input.SecondCoinId).FirstOrDefault();

                _unitOfWork.CreateTransaction();


                var exchange = new Exchange()
                {
                    BranchId = BranchHelper.Id,
                    FirstCoinId = input.FirstCoinId,
                    SecoundCoinId = input.SecondCoinId,
                    AmountOfFirstCoin = input.AmountOfFirstCoin,
                    AmoutOfSecoundCoin = input.AmoutOfSecondCoin,
                    MainCoinId = mainCoin.CoinId,
                    FirstCoinExchangePriceWithMainCoin = firstCoinExchange.ExchangePrice,
                    FirstCoinSellingPriceWithMainCoin = firstCoinExchange.SellingPrice,
                    FirstCoinPurchasingPriceWithMainCoin = firstCoinExchange.PurchasingPrice,
                    SecoundCoinExchangePriceWithMainCoin = secoundCoinExchange.ExchangePrice,
                    SecoundCoinSellingPricWithMainCoin = secoundCoinExchange.SellingPrice,
                    SecoundCoinPurchasingPriceWithMainCoin = secoundCoinExchange.PurchasingPrice
                };
                _unitOfWork.GenericRepository<Exchange>().Insert(exchange);

                var moneyAction = new MoneyAction()
                {
                    ExchangeId = exchange.Id,
                    Date = input.Date
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                
                var branchCashFlowForFirstCoin = new BranchCashFlow()
                {
                    BranchId = BranchHelper.Id,
                    MoenyAction = moneyAction,
                    CoinId = input.FirstCoinId,
                    Amount = firstAmount,
                    TreasuryId = treasuryId,
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlowForFirstCoin);
                
                TreasuryMoneyAction sellingCoinTreasuryMoeyAction = new TreasuryMoneyAction()
                {
                    Amount = firstAmount,
                    CoinId = input.FirstCoinId,
                    TreasuryId = treasuryId,
                    BranchCashFlow = branchCashFlowForFirstCoin,
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(sellingCoinTreasuryMoeyAction);
                var mainTreasuryId = _appSession.GetMainTreasury();
                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = firstAmount,
                        CoinId = input.FirstCoinId,
                        BranchCashFlow = branchCashFlowForFirstCoin,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
                }

                
                var branChCashFlowForSecoundCoin = new BranchCashFlow()
                {
                    BranchId = BranchHelper.Id,
                    CoinId = input.SecondCoinId,
                    MonyActionId = moneyAction.Id,
                    Amount = secondAmount,
                    TreasuryId = treasuryId,
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branChCashFlowForSecoundCoin);

                TreasuryMoneyAction secoundCoinTreasuryMoeyAction = new TreasuryMoneyAction()
                {
                    Amount = secondAmount,
                    CoinId = input.SecondCoinId,
                    TreasuryId = treasuryId,
                    BranchCashFlow = branChCashFlowForSecoundCoin,
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(secoundCoinTreasuryMoeyAction);
                
                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyActionSecoundCoin = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = secondAmount,
                        BranchCashFlow = branChCashFlowForSecoundCoin,
                        CoinId = input.SecondCoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyActionSecoundCoin);
                }
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        public bool ExchangeForClient(ExchangeInputDto input)
        {
            try
            {
                int treasuryId = _appSession.GetCurrentTreasuryId();
                var firstAmount = input.ActionType == 1 ? -input.AmountOfFirstCoin : input.AmountOfFirstCoin;
                var secondAmount = input.ActionType == 1 ? input.AmoutOfSecondCoin : -input.AmoutOfSecondCoin;

                var mainCoin = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.IsMainCoin == true).FirstOrDefault();
                var firstCoinExchange = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == input.FirstCoinId).FirstOrDefault();
                var secoundCoinExchange = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == input.SecondCoinId).FirstOrDefault();
                _unitOfWork.CreateTransaction();


                var exchange = new Exchange()
                {
                    BranchId = BranchHelper.Id,
                    FirstCoinId = input.FirstCoinId,
                    SecoundCoinId = input.SecondCoinId,
                    AmountOfFirstCoin = input.AmountOfFirstCoin,
                    AmoutOfSecoundCoin = input.AmoutOfSecondCoin,
                    MainCoinId = mainCoin.CoinId,
                };

                _unitOfWork.GenericRepository<Exchange>().Insert(exchange);
                                              
                var moneyAction = new MoneyAction()
                {
                    ExchangeId = exchange.Id,
                    Date = input.Date
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var clientCashFlowForFirstCoin = new ClientCashFlow()
                {
                    ClientId = input.AgentId.Value,
                    MoenyAction = moneyAction,
                    CoinId = input.FirstCoinId,
                    
                    Amount = -firstAmount
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlowForFirstCoin);
                             
                var clientCashFlowForSecondCoin = new ClientCashFlow()
                {
                    ClientId = input.AgentId.Value,
                    CoinId = input.SecondCoinId,
                    MoenyAction = moneyAction,
                    
                    Amount = secondAmount
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlowForSecondCoin);

                _unitOfWork.Save();
                _unitOfWork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        public bool ExchangeForCompany(ExchangeInputDto input)
        {
            try
            {
                int treasuryId = _appSession.GetCurrentTreasuryId();
                var firstAmount = input.ActionType == 1 ? -input.AmountOfFirstCoin : input.AmountOfFirstCoin;
                var secondAmount = input.ActionType == 1 ? input.AmoutOfSecondCoin : -input.AmoutOfSecondCoin;

                var mainCoin = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.IsMainCoin == true).FirstOrDefault();
                var firstCoinExchange = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == input.FirstCoinId).FirstOrDefault();
                var secoundCoinExchange = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == input.SecondCoinId).FirstOrDefault();
                _unitOfWork.CreateTransaction();


                var exchange = new Exchange()
                {
                    BranchId = BranchHelper.Id,
                    FirstCoinId = input.FirstCoinId,
                    SecoundCoinId = input.SecondCoinId,
                    AmountOfFirstCoin = firstAmount,
                    AmoutOfSecoundCoin = secondAmount,
                    MainCoinId = mainCoin.CoinId,
                };

                _unitOfWork.GenericRepository<Exchange>().Insert(exchange);

                var moneyAction = new MoneyAction()
                {
                    ExchangeId = exchange.Id,
                    Date = input.Date
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var companyCashFlowForFirstCoin = new CompanyCashFlow()
                {
                    CompanyId = input.CompanyId.Value,
                    MoenyAction = moneyAction,
                    CoinId = input.FirstCoinId,
                    
                    Amount = firstAmount
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlowForFirstCoin);

                var companyCashFlowForSecondCoin = new CompanyCashFlow()
                {
                    CompanyId = input.CompanyId.Value,
                    CoinId = input.SecondCoinId,
                    MoenyAction = moneyAction,
                    
                    Amount = secondAmount
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlowForSecondCoin);

                _unitOfWork.Save();
                _unitOfWork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        public decimal CalcForFirstCoin(int sellingCoinId, int purchasingCoinId, decimal amount)
        {
            decimal value = 0;
            var mainCoin = _unitOfWork.GenericRepository<BranchCash>().GetAll().Where(c => c.IsMainCoin).FirstOrDefault();
            var branchCashes = _unitOfWork.GenericRepository<BranchCash>().GetAll();
            if (sellingCoinId == mainCoin.CoinId)
            {
                var branchCash = branchCashes.Where(c => c.CoinId == purchasingCoinId).FirstOrDefault();
                if (branchCash != null)
                {
                    value = CalcFromMainCoin(branchCash, amount);
                }
            }
            else if (purchasingCoinId == mainCoin.CoinId)
            {
                var branchCash = branchCashes.Where(c => c.CoinId == sellingCoinId).FirstOrDefault();
                if (branchCash != null)
                {
                    value = CalcToMainCoin(branchCash, amount);
                }
            }
            else
            {
                var firstCoin = branchCashes.Where(c => c.CoinId == sellingCoinId).FirstOrDefault();
                var seconcCoin = branchCashes.Where(c => c.CoinId == purchasingCoinId).FirstOrDefault();
                var firstCoinAmount = CalcToMainCoin(firstCoin, amount);
                value= CalcFromMainCoin(seconcCoin, firstCoinAmount);
            }

            return Math.Round(value, 3);
        }

        private decimal CalcToMainCoin(BranchCash branchCash, decimal amount)
        {
            return (branchCash.ExchangePrice != null && branchCash.ExchangePrice != 0) ? amount / branchCash.ExchangePrice.Value : 0;
        }

        private decimal CalcFromMainCoin(BranchCash branchCash, decimal amount)
        {
            return (branchCash.ExchangePrice != null && branchCash.ExchangePrice != 0) ? amount * branchCash.ExchangePrice.Value : 0;
        }

        //public decimal CalcForFirstCoin(int sellingCoinId, int purchasingCoinId, decimal amountFromFirstCoin)
        //{
        //    decimal value = 0;
        //    var mainCoin = _unitOfWork.GenericRepository<BranchCash>().GetAll().Where(c => c.IsMainCoin).FirstOrDefault();
        //    var branchCashes = _unitOfWork.GenericRepository<BranchCash>().GetAll();
        //    if (sellingCoinId == mainCoin.CoinId)
        //    {
        //        var branchCash = branchCashes.Where(c => c.CoinId == purchasingCoinId).FirstOrDefault();
        //        if (branchCash != null)
        //        {
        //            value = (decimal)((branchCash.SellingPrice ?? 0) * amountFromFirstCoin);
        //        }
        //    }
        //    else if (purchasingCoinId == mainCoin.CoinId)
        //    {
        //        var branchCash = branchCashes.Where(c => c.CoinId == sellingCoinId).FirstOrDefault();
        //        if (branchCash != null && branchCash.PurchasingPrice != 0)
        //        {
        //            value = (decimal)(amountFromFirstCoin * (branchCash.PurchasingPrice ?? 0));
        //        }
        //    }
        //    else
        //    {
        //        var fromFirstCoinForMainCoin = (amountFromFirstCoin / branchCashes.Where(c => c.CoinId == sellingCoinId).FirstOrDefault().PurchasingPrice ?? 1);
        //        var fromMainCoinForSecounCoin = (branchCashes.Where(c => c.CoinId == purchasingCoinId).FirstOrDefault().SellingPrice ?? 1 * fromFirstCoinForMainCoin);
        //        return fromMainCoinForSecounCoin;
        //    }

        //    return Math.Round(value, 1);
        //}
    }
}
