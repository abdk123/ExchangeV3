using BWR.Application.Dtos.Branch;
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

        public bool ExchangeForBranch(int sellingCoinId, int purchasingCoinId, decimal firstAmount)
        {
            try
            {
                int treasuryId = _appSession.GetCurrentTreasuryId();
                var secondCoinAmount = CalcForFirstCoin(sellingCoinId, purchasingCoinId, firstAmount);

                var mainCoin = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.IsMainCoin == true).FirstOrDefault();
                var firstCoinExchange = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == sellingCoinId).FirstOrDefault();
                var secoundCoinExchange = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == purchasingCoinId).FirstOrDefault();


                _unitOfWork.CreateTransaction();


                var exchange = new Exchange()
                {
                    BranchId = BranchHelper.Id,
                    FirstCoinId = sellingCoinId,
                    SecoundCoinId = purchasingCoinId,
                    AmountOfFirstCoin = firstAmount,
                    AmoutOfSecoundCoin = secondCoinAmount,
                    MainCoinId = mainCoin.CoinId,
                    FirstCoinExchangePriceWithMainCoin = firstCoinExchange.ExchangePrice,
                    FirstCoinSellingPriceWithMainCoin = firstCoinExchange.SellingPrice,
                    FirstCoinPurchasingPriceWithMainCoin = firstCoinExchange.PurchasingPrice,
                    SecoundCoinExchangePriceWithMainCoin = secoundCoinExchange.ExchangePrice,
                    SecoundCoinSellingPricWithMainCoin = secoundCoinExchange.SellingPrice,
                    SecoundCoinPurchasingPriceWithMainCoin = secoundCoinExchange.PurchasingPrice
                };
                _unitOfWork.GenericRepository<Exchange>().Insert(exchange);


                var branchCaches = _unitOfWork.GenericRepository<BranchCash>();
                var branchCashSellingCoin = branchCaches.FindBy(c => c.CoinId == sellingCoinId).FirstOrDefault();
                branchCashSellingCoin.Total -= firstAmount;
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCashSellingCoin);


                var branchCashpurhesCoin = branchCaches.FindBy(c => c.CoinId == purchasingCoinId).FirstOrDefault();
                branchCashpurhesCoin.Total += secondCoinAmount;
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCashpurhesCoin);



                var moneyAction = new MoneyAction()
                {
                    ExchangeId = exchange.Id
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var branchCashFlowForFirstCoin = new BranchCashFlow()
                {
                    BranchId = BranchHelper.Id,
                    MoenyAction = moneyAction,
                    CoinId = sellingCoinId,
                    Total = branchCashSellingCoin.Total,
                    Amount = -firstAmount,
                    TreasuryId = treasuryId,
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlowForFirstCoin);
                var sellingCoinTreasuryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(C => C.TreasuryId == treasuryId && C.CoinId == sellingCoinId).FirstOrDefault();
                sellingCoinTreasuryCash.Total -= firstAmount;
                _unitOfWork.GenericRepository<TreasuryCash>().Update(sellingCoinTreasuryCash);
                TreasuryMoneyAction sellingCoinTreasuryMoeyAction = new TreasuryMoneyAction()
                {
                    Amount = -firstAmount,
                    CoinId = sellingCoinId,
                    TreasuryId = treasuryId,
                    Total = sellingCoinTreasuryCash.Total,
                    BranchCashFlow = branchCashFlowForFirstCoin,
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(sellingCoinTreasuryMoeyAction);
                var mainTreasuryId = _appSession.GetMainTreasury();
                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = -firstAmount,
                        CoinId = sellingCoinId,
                        BranchCashFlow = branchCashFlowForFirstCoin,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
                }


                var branChCashFlowForSecoundCoin = new BranchCashFlow()
                {
                    BranchId = BranchHelper.Id,
                    CoinId = purchasingCoinId,
                    MonyActionId = moneyAction.Id,
                    Total = branchCashpurhesCoin.Total,
                    Amount = secondCoinAmount,
                    TreasuryId = treasuryId,
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branChCashFlowForSecoundCoin);

                var secoundCoinTreasuryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(C => C.TreasuryId == treasuryId && C.CoinId == sellingCoinId).FirstOrDefault();
                secoundCoinTreasuryCash.Total += secondCoinAmount;
                _unitOfWork.GenericRepository<TreasuryCash>().Update(secoundCoinTreasuryCash);
                TreasuryMoneyAction secoundCoinTreasuryMoeyAction = new TreasuryMoneyAction()
                {
                    Amount = +secondCoinAmount,
                    CoinId = purchasingCoinId,
                    TreasuryId = treasuryId,
                    Total = secoundCoinTreasuryCash.Total,
                    BranchCashFlow = branChCashFlowForSecoundCoin,
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(secoundCoinTreasuryMoeyAction);
                
                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyActionSecoundCoin = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = +secondCoinAmount,
                        BranchCashFlow = branChCashFlowForSecoundCoin,
                        CoinId = purchasingCoinId,
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

        public bool ExchangeForClient(int clientId, int sellingCoinId, int purchasingCoinId, decimal firstAmount)
        {
            try
            {
                int treasuryId = _appSession.GetCurrentTreasuryId();
                var secondCoinAmount = CalcForFirstCoin(sellingCoinId, purchasingCoinId, firstAmount);
                var mainCoin = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.IsMainCoin == true).FirstOrDefault();
                var firstCoinExchange = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.CoinId == sellingCoinId && c.ClientId == clientId).FirstOrDefault();
                var secoundCoinExchange = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.CoinId == purchasingCoinId && c.ClientId == clientId).FirstOrDefault();

                _unitOfWork.CreateTransaction();


                var exchange = new Exchange()
                {
                    BranchId = BranchHelper.Id,
                    FirstCoinId = sellingCoinId,
                    SecoundCoinId = purchasingCoinId,
                    AmountOfFirstCoin = firstAmount,
                    AmoutOfSecoundCoin = secondCoinAmount,
                    MainCoinId = mainCoin.CoinId,
                };

                _unitOfWork.GenericRepository<Exchange>().Insert(exchange);


                var clientCaches = _unitOfWork.GenericRepository<ClientCash>().FindBy(c=> c.ClientId == clientId);
                var branchCashSellingCoin = clientCaches.Where(c => c.CoinId == sellingCoinId).FirstOrDefault();
                branchCashSellingCoin.Total -= firstAmount;
                _unitOfWork.GenericRepository<ClientCash>().Update(branchCashSellingCoin);


                var branchCashpurhesCoin = clientCaches.Where(c => c.CoinId == purchasingCoinId).FirstOrDefault();
                branchCashpurhesCoin.Total += secondCoinAmount;
                _unitOfWork.GenericRepository<ClientCash>().Update(branchCashpurhesCoin);

                var moneyAction = new MoneyAction()
                {
                    ExchangeId = exchange.Id
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var clientCashFlowForFirstCoin = new ClientCashFlow()
                {
                    ClientId = clientId,
                    MoenyAction = moneyAction,
                    CoinId = sellingCoinId,
                    Total = branchCashSellingCoin.Total,
                    Amount = -firstAmount
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlowForFirstCoin);
                             
                var clientCashFlowForSecondCoin = new ClientCashFlow()
                {
                    ClientId = clientId,
                    CoinId = purchasingCoinId,
                    MoenyAction = moneyAction,
                    Total = branchCashpurhesCoin.Total,
                    Amount = secondCoinAmount
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

        public bool ExchangeForCompany(int companyId, int sellingCoinId, int purchasingCoinId, decimal firstAmount)
        {
            try
            {
                int treasuryId = _appSession.GetCurrentTreasuryId();
                var secondCoinAmount = CalcForFirstCoin(sellingCoinId, purchasingCoinId, firstAmount);
                var mainCoin = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.IsMainCoin == true).FirstOrDefault();
                var firstCoinExchange = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CoinId == sellingCoinId && c.CompanyId == companyId).FirstOrDefault();
                var secoundCoinExchange = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CoinId == purchasingCoinId && c.CompanyId == companyId).FirstOrDefault();

                _unitOfWork.CreateTransaction();


                var exchange = new Exchange()
                {
                    BranchId = BranchHelper.Id,
                    FirstCoinId = sellingCoinId,
                    SecoundCoinId = purchasingCoinId,
                    AmountOfFirstCoin = firstAmount,
                    AmoutOfSecoundCoin = secondCoinAmount,
                    MainCoinId = mainCoin.CoinId,
                };

                _unitOfWork.GenericRepository<Exchange>().Insert(exchange);


                var companyCaches = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == companyId);
                var branchCashSellingCoin = companyCaches.Where(c => c.CoinId == sellingCoinId).FirstOrDefault();
                branchCashSellingCoin.Total -= firstAmount;
                _unitOfWork.GenericRepository<CompanyCash>().Update(branchCashSellingCoin);


                var branchCashpurhesCoin = companyCaches.Where(c => c.CoinId == purchasingCoinId).FirstOrDefault();
                branchCashpurhesCoin.Total += secondCoinAmount;
                _unitOfWork.GenericRepository<CompanyCash>().Update(branchCashpurhesCoin);

                var moneyAction = new MoneyAction()
                {
                    ExchangeId = exchange.Id
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var companyCashFlowForFirstCoin = new CompanyCashFlow()
                {
                    CompanyId = companyId,
                    MoenyAction = moneyAction,
                    CoinId = sellingCoinId,
                    Total = branchCashSellingCoin.Total,
                    Amount = -firstAmount
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlowForFirstCoin);

                var companyCashFlowForSecondCoin = new CompanyCashFlow()
                {
                    CompanyId = companyId,
                    CoinId = purchasingCoinId,
                    MoenyAction = moneyAction,
                    Total = branchCashpurhesCoin.Total,
                    Amount = secondCoinAmount
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
