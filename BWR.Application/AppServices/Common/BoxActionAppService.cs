using BWR.Application.Dtos.BoxAction;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Client;
using BWR.Application.Dtos.Company;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Dtos.Setting.PublicExpense;
using BWR.Application.Dtos.Setting.PublicIncome;
using BWR.Application.Interfaces.BoxAction;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Enums;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Treasures;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.AppServices.BoxActions
{
    public class BoxActionAppService : IBoxActionAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public BoxActionAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public BoxActionUpdateDto GetForEdit(int moneyActionId)
        {
            BoxActionUpdateDto dto = null;
            try
            {
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().GetById(moneyActionId);
                
                if (moneyAction.BoxAction != null)
                {
                    var boxAction = moneyAction.BoxAction;

                    dto = new BoxActionUpdateDto()
                    {
                        Amount = boxAction.Amount,
                        CoinId = boxAction.CoinId,
                        IsIncome = boxAction.IsIncmoe,
                        ExpensiveId = moneyAction.PublicMoney != null ? moneyAction.PublicMoney.ExpenseId : null,
                        IncomeId = moneyAction.PublicMoney != null ? moneyAction.PublicMoney.IncomeId : null,
                        Note = boxAction.Note,
                        MoneyActionId = moneyAction.Id,
                        BoxActionType = boxAction.BoxActionType.ToString()
                    };

                    if (moneyAction.CompanyCashFlows.Any())
                    {
                        dto.FirstCompanyId = moneyAction.CompanyCashFlows.FirstOrDefault().CompanyId;
                    }

                    if (moneyAction.ClientCashFlows.Any())
                    {
                        dto.FirstClientId = moneyAction.ClientCashFlows.FirstOrDefault().ClientId;
                    }
                }
                else if(moneyAction.Clearing != null)
                {
                    var clearing = moneyAction.Clearing;
                    dto = new BoxActionUpdateDto()
                    {
                        Amount = clearing.Amount,
                        CoinId = clearing.CoinId,
                        Note = clearing.Note,
                        IsIncome = clearing.IsIncome,
                        MoneyActionId = moneyAction.Id,
                        BoxActionType = BoxActionType.None.ToString(),
                        FirstClientId = moneyAction.Clearing.FromClientId,
                        SecondClientId = moneyAction.Clearing.ToClientId,
                        FirstCompanyId = moneyAction.Clearing.FromCompanyId,
                        SecondCompanyId = moneyAction.Clearing.ToCompanyId
                    };
                }
                
            }
            catch (Exception)
            {

            }

            return dto;
        }

        #region Insert
        public bool ExpenseFromTreasury(BoxActionExpensiveDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var publicActionMoneyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.ExpenseId == input.ExpensiveId).FirstOrDefault().Id;
                var branchId = BranchHelper.Id;

                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.BranchId == branchId && c.CoinId == input.CoinId).First();

                branchCash.Total -= input.Amount;
                branchCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);

                var treasuryId = _appSession.GetCurrentTreasuryId();
                var mainTreasuryId = _appSession.GetMainTreasury();
                var treasuryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(c => c.CoinId == input.CoinId && c.TreasuryId == treasuryId).FirstOrDefault();
                treasuryCash.Total -= input.Amount;
                treasuryCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treasuryCash);

                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    CoinId = input.CoinId,
                    IsIncmoe = false,
                    Note = input.Note,
                    BoxActionType = BoxActionType.ExpenseFromTreasury,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);

                var moneyAction = new MoneyAction()
                {
                    PubLicMoneyId = publicActionMoneyId,
                    BoxAction = boxAction,
                    CreatedBy = _appSession.GetUserName(),
                    Date = DateTime.Now
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Total = branchCash.Total,
                    Amount = -input.Amount,
                    MoenyAction = moneyAction,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);

                var treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    Amount = -input.Amount,
                    CoinId = input.CoinId,
                    TreasuryId = treasuryId,
                    Total = treasuryCash.Total,
                    BranchCashFlow = branchCashFlow,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        Amount = -input.Amount,
                        CoinId = input.CoinId,
                        TreasuryId = mainTreasuryId,
                        BranchCashFlow = branchCashFlow,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
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

        public bool ReceiveToTreasury(BoxActionIncomeDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var publicActionMoneyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.IncomeId == input.IncomeId).FirstOrDefault().Id;

                var branchId = BranchHelper.Id;

                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.BranchId == branchId && c.CoinId == input.CoinId).FirstOrDefault();

                branchCash.Total += input.Amount;
                branchCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);

                var treasuryId = _appSession.GetCurrentTreasuryId();
                var mainTreasuryId = _appSession.GetMainTreasury();
                var treuseryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(c => c.CoinId == input.CoinId && c.TreasuryId == treasuryId).FirstOrDefault();
                treuseryCash.Total += input.Amount;
                treuseryCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treuseryCash);

                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    CoinId = input.CoinId,
                    IsIncmoe = true,
                    Note = input.Note,
                    BoxActionType=BoxActionType.ReceiveToTreasury,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);

                var moneyAction = new MoneyAction()
                {
                    PubLicMoneyId = publicActionMoneyId,
                    BoxAction = boxAction,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);

                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Total = branchCash.Total,
                    Amount = input.Amount,
                    MoenyAction = moneyAction,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);

                TreasuryMoneyAction treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    TreasuryId = treasuryId,
                    Amount = input.Amount,
                    Total = treuseryCash.Total,
                    BranchCashFlow = branchCashFlow,
                    CoinId = input.CoinId,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = input.Amount,
                        BranchCashFlow = branchCashFlow,
                        CoinId = input.CoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
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

        public bool ExpenseFromTreasuryToClient(BoxActionClientDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();


                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var mainTreasuryId = _appSession.GetMainTreasury();

                var treasuryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(c => c.CoinId == input.CoinId && c.TreasuryId == treasuryId).FirstOrDefault();
                treasuryCash.Total -= input.Amount;
                treasuryCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treasuryCash);


                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.BranchId == branchId && c.CoinId == input.CoinId).FirstOrDefault();
                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    IsIncmoe = false,
                    CoinId = input.CoinId,
                    Note = input.Note,
                    BoxActionType = BoxActionType.ExpenseFromTreasuryToClient,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
                    Date = DateTime.Now,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                branchCash.Total -= input.Amount;
                branchCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);


                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Total = branchCash.Total,
                    Amount = -input.Amount,
                    MoenyAction = moneyAction,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);
                TreasuryMoneyAction treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    TreasuryId = treasuryId,
                    CoinId = input.CoinId,
                    Amount = -input.Amount,
                    Total = treasuryCash.Total,
                    BranchCashFlow = branchCashFlow,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = -input.Amount,
                        BranchCashFlow = branchCashFlow,
                        CoinId = input.CoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
                }

                var clientCash = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == input.ClientId && c.CoinId == input.CoinId).FirstOrDefault();
                clientCash.Total -= input.Amount;
                clientCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);

                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = input.CoinId,
                    ClientId = input.ClientId,
                    Total = clientCash.Total,
                    Amount = -input.Amount,
                    MoenyAction = moneyAction,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);

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

        public bool ReceiveFromClientToTreasury(BoxActionClientDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var mainTreasuryId = _appSession.GetMainTreasury();

                var treuseryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(c => c.CoinId == input.CoinId && c.TreasuryId == treasuryId).FirstOrDefault();
                treuseryCash.Total += input.Amount;
                treuseryCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treuseryCash);

                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.BranchId == branchId && c.CoinId == input.CoinId).FirstOrDefault();
                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    IsIncmoe = true,
                    CoinId = input.CoinId,
                    Note = input.Note,
                    BoxActionType = BoxActionType.ReceiveFromClientToTreasury,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
                    Date = DateTime.Now,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);

                branchCash.Total += input.Amount;
                branchCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);

                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Total = branchCash.Total,
                    Amount = input.Amount,
                    MonyActionId = moneyAction.Id,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);
                TreasuryMoneyAction treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    TreasuryId = treasuryId,
                    BranchCashFlow = branchCashFlow,
                    Amount = input.Amount,
                    Total = treuseryCash.Total,
                    CoinId = input.CoinId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = input.Amount,
                        BranchCashFlow = branchCashFlow,
                        CoinId = input.CoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
                }

                var clientCash = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == input.ClientId && c.CoinId == input.CoinId).FirstOrDefault();
                clientCash.Total += input.Amount;
                clientCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);


                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = input.CoinId,
                    ClientId = input.ClientId,
                    Total = clientCash.Total,
                    Amount = input.Amount,
                    MoenyAction = moneyAction,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);

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

        public bool ReceiveFromCompanyToTreasury(BoxActionCompanyDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var mainTreasuryId = _appSession.GetMainTreasury();

                var treuseryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(c => c.CoinId == input.CoinId && c.TreasuryId == treasuryId).FirstOrDefault();
                treuseryCash.Total += input.Amount;
                treuseryCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treuseryCash);

                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.BranchId == branchId && c.CoinId == input.CoinId).FirstOrDefault();
                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    IsIncmoe = true,
                    CoinId = input.CoinId,
                    Note = input.Note,
                    BoxActionType = BoxActionType.ReceiveFromCompanyToTreasury,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
                    Date = DateTime.Now,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);

                branchCash.Total += input.Amount;
                branchCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);

                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Total = branchCash.Total,
                    Amount = input.Amount,
                    MonyActionId = moneyAction.Id,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);

                TreasuryMoneyAction treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    TreasuryId = treasuryId,
                    CoinId = input.CoinId,
                    Amount = input.Amount,
                    Total = treuseryCash.Total,
                    BranchCashFlow = branchCashFlow,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = input.Amount,
                        BranchCashFlow = branchCashFlow,
                        CoinId = input.CoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
                }

                var companyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == input.CompanyId && c.CoinId == input.CoinId).FirstOrDefault();
                companyCash.Total += input.Amount;
                _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);

                var companyCashFlow = new CompanyCashFlow()
                {
                    CoinId = input.CoinId,
                    CompanyId = input.CompanyId,
                    Amount = input.Amount,
                    Total = companyCash.Total,
                    MoenyAction = moneyAction,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);

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

        public bool ExpenseFromTreasuryToCompany(BoxActionCompanyDto input)
        {
            try
            {
                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();

                var treasuryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(c => c.CoinId == input.CoinId && c.TreasuryId == treasuryId).FirstOrDefault();
                _unitOfWork.CreateTransaction();
                treasuryCash.Total -= input.Amount;
                treasuryCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treasuryCash);

                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.BranchId == branchId && c.CoinId == input.CoinId).FirstOrDefault();
                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    IsIncmoe = false,
                    CoinId = input.CoinId,
                    Note = input.Note,
                    BoxActionType = BoxActionType.ExpenseFromTreasuryToCompany,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
                    Date = DateTime.Now,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                branchCash.Total -= input.Amount;
                branchCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);

                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Total = branchCash.Total,
                    Amount = -input.Amount,
                    MoenyAction = moneyAction,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);
                TreasuryMoneyAction treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    TreasuryId = treasuryId,
                    Amount = -input.Amount,
                    BranchCashFlow = branchCashFlow,
                    Total = treasuryCash.Total,
                    CoinId = input.CoinId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                var mainTreasuryId = _appSession.GetMainTreasury();
                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = -input.Amount,
                        BranchCashFlow = branchCashFlow,
                        CoinId = input.CoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
                }

                var companyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == input.CompanyId && c.CoinId == input.CoinId).FirstOrDefault();
                companyCash.Total -= input.Amount;
                companyCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);

                var companyCashFlow = new CompanyCashFlow()
                {
                    CoinId = input.CoinId,
                    Amount = -input.Amount,
                    CompanyId = input.CompanyId,
                    Total = companyCash.Total,
                    MoenyAction = moneyAction,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);

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

        public bool FromClientToClient(BoxActionFromClientToClientDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var firstClientChash = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == dto.FirstClientId && c.CoinId == dto.CoinId).First();
                firstClientChash.Total += dto.Amount;
                _unitOfWork.GenericRepository<ClientCash>().Update(firstClientChash);
                var secounClientCahs = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == dto.SecondClientId && c.CoinId == dto.CoinId).First();
                secounClientCahs.Total += (dto.Amount * -1);
                _unitOfWork.GenericRepository<ClientCash>().Update(secounClientCahs);
                Clearing clearing = new Clearing()
                {
                    FromClientId = dto.FirstClientId,
                    ToClientId = dto.SecondClientId,
                    IsIncome = dto.Amount > 0,
                    CoinId = dto.CoinId,
                    Note = dto.Note
                };
                _unitOfWork.GenericRepository<Clearing>().Insert(clearing);
                var moenyAction = new MoneyAction()
                {
                    ClearingId = clearing.Id
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moenyAction);
                ClientCashFlow firstClientCashFlow = new ClientCashFlow()
                {
                    ClientId = dto.FirstClientId,
                    CoinId = dto.CoinId,
                    MoenyActionId = moenyAction.Id,
                    Amount = dto.Amount,
                    Total = firstClientChash.Total,
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(firstClientCashFlow);

                ClientCashFlow SecoundClientCashFlow = new ClientCashFlow()
                {
                    ClientId = dto.SecondClientId,
                    CoinId = dto.CoinId,
                    MoenyActionId = moenyAction.Id,
                    Amount = -dto.Amount,
                    Total = secounClientCahs.Total,
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(SecoundClientCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        public bool FromCompanyToClient(BoxActionFromCompanyToClientDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var companyChash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == dto.CompanyId && c.CoinId == dto.CoinId).First();
                companyChash.Total += dto.Amount;
                _unitOfWork.GenericRepository<CompanyCash>().Update(companyChash);
                var clientCash = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == dto.ClientId && c.CoinId == dto.CoinId).First();
                clientCash.Total += (dto.Amount * -1);
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);
                Clearing clearing = new Clearing()
                {
                    FromCompanyId = dto.CompanyId,
                    ToClientId = dto.ClientId,
                    IsIncome = dto.Amount > 0,
                    CoinId = dto.CoinId,
                    Note = dto.Note
                };
                _unitOfWork.GenericRepository<Clearing>().Insert(clearing);
                var moenyAction = new MoneyAction()
                {
                    ClearingId = clearing.Id
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moenyAction);
                CompanyCashFlow companyCashFlow = new CompanyCashFlow()
                {
                    CompanyId = dto.CompanyId,
                    CoinId = dto.CoinId,
                    MoneyActionId = moenyAction.Id,
                    Amount = dto.Amount,
                    Total = companyChash.Total,
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);

                ClientCashFlow clientCashFlow = new ClientCashFlow()
                {
                    ClientId = dto.ClientId,
                    CoinId = dto.CoinId,
                    MoenyActionId = moenyAction.Id,
                    Amount = -dto.Amount,
                    Total = clientCash.Total,
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        public bool FromClientToCompany(BoxActionFromClientToCompanyDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var clientChash = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == dto.ClientId && c.CoinId == dto.CoinId).First();
                clientChash.Total += dto.Amount;
                _unitOfWork.GenericRepository<ClientCash>().Update(clientChash);
                var companyChash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == dto.CompanyId && c.CoinId == dto.CoinId).First();
                companyChash.Total += (dto.Amount * -1);
                _unitOfWork.GenericRepository<CompanyCash>().Update(companyChash);
                Clearing clearing = new Clearing()
                {
                    FromClientId = dto.ClientId,
                    ToCompanyId = dto.CompanyId,
                    IsIncome = dto.Amount > 0,
                    CoinId = dto.CoinId,
                    Note = dto.Note
                };
                _unitOfWork.GenericRepository<Clearing>().Insert(clearing);
                var moenyAction = new MoneyAction()
                {
                    ClearingId = clearing.Id
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moenyAction);
                ClientCashFlow clientCashFlow = new ClientCashFlow()
                {
                    ClientId = dto.ClientId,
                    CoinId = dto.CoinId,
                    MoenyActionId = moenyAction.Id,
                    Amount = dto.Amount,
                    Total = clientChash.Total,
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);

                CompanyCashFlow companyCashFlow = new CompanyCashFlow()
                {
                    CompanyId = dto.CompanyId,
                    CoinId = dto.CoinId,
                    MoneyActionId = moenyAction.Id,
                    Amount = -dto.Amount,
                    Total = companyChash.Total,
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        public bool FromCompanyToCompany(BoxActionFromCompanyToCompanyDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var firstCompanyChash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == dto.FirstCompanyId && c.CoinId == dto.CoinId).First();
                firstCompanyChash.Total += dto.Amount;
                _unitOfWork.GenericRepository<CompanyCash>().Update(firstCompanyChash);
                var secounCompanyCahs = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == dto.SecondCompanyId && c.CoinId == dto.CoinId).First();
                secounCompanyCahs.Total += (dto.Amount * -1);
                _unitOfWork.GenericRepository<CompanyCash>().Update(secounCompanyCahs);
                Clearing clearing = new Clearing()
                {
                    FromCompanyId = dto.FirstCompanyId,
                    ToCompanyId = dto.SecondCompanyId,
                    IsIncome = dto.Amount > 0,
                    CoinId = dto.CoinId,
                    Note = dto.Note
                };
                _unitOfWork.GenericRepository<Clearing>().Insert(clearing);
                var moenyAction = new MoneyAction()
                {
                    ClearingId = clearing.Id
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moenyAction);
                CompanyCashFlow firstCompanyCahsFlwo = new CompanyCashFlow()
                {
                    CompanyId = dto.FirstCompanyId,
                    CoinId = dto.CoinId,
                    MoneyActionId = moenyAction.Id,
                    Amount = dto.Amount,
                    Total = firstCompanyChash.Total,
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(firstCompanyCahsFlwo);

                CompanyCashFlow SecoundCompanyCashFlow = new CompanyCashFlow()
                {
                    CompanyId = dto.SecondCompanyId,
                    CoinId = dto.CoinId,
                    MoneyActionId = moenyAction.Id,
                    Amount = -dto.Amount,
                    Total = secounCompanyCahs.Total,
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(SecoundCompanyCashFlow);
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

        public bool ExpenseFromClientToPublic(BoxActionFromClientToPublicExpenesDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var publicMoenyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.ExpenseId == dto.PublicExpenseId).First().Id;
                var boxAction = new BoxAction()
                {
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    IsIncmoe = false,
                    BoxActionType = BoxActionType.ExpenseFromClientToPublic,
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    Date = DateTime.Now,
                    PubLicMoneyId = publicMoenyId
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var clientCash = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == dto.ClientId && c.CoinId == dto.CoinId).Single();
                clientCash.Total += dto.Amount;
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);
                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    ClientId = dto.ClientId,
                    Total = clientCash.Total,
                    MoenyActionId = moneyAction.Id,
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                Tracing.SaveException(ex);
                return false;
            }
        }

        public bool ReceiveFromPublicToClient(BoxActionFromClientToPublicIncomeDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var publicMoenyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.IncomeId == dto.PublicIncomeId).First().Id;
                var boxAction = new BoxAction()
                {
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    IsIncmoe = true,
                    BoxActionType = BoxActionType.ReceiveFromPublicToClient,
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    Date = DateTime.Now,
                    PubLicMoneyId = publicMoenyId
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var clientCash = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == dto.ClientId && c.CoinId == dto.CoinId).Single();
                clientCash.Total -= dto.Amount;
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);
                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = dto.CoinId,
                    Amount = -dto.Amount,
                    ClientId = dto.ClientId,
                    Total = clientCash.Total,
                    MoenyActionId = moneyAction.Id,
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                Tracing.SaveException(ex);
                return false;
            }
        }

        public bool ExpenseFromCompanyToPublic(BoxActionFromCompanyToPublicExpenesDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var publicMoenyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.ExpenseId == dto.PublicExpenseId).First().Id;
                var boxAction = new BoxAction()
                {
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    IsIncmoe = false,
                    BoxActionType = BoxActionType.ExpenseFromCompanyToPublic,
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    Date = DateTime.Now,
                    PubLicMoneyId = publicMoenyId
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var companyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == dto.CompanyId && c.CoinId == dto.CoinId).Single();
                companyCash.Total += dto.Amount;
                _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);
                var companyCashFlow = new CompanyCashFlow()
                {
                    CoinId = dto.CoinId,
                    Amount = +dto.Amount,
                    CompanyId = dto.CompanyId,
                    Total = companyCash.Total,
                    MoneyActionId = moneyAction.Id,
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                Tracing.SaveException(ex);
                return false;
            }
        }

        public bool ReceiveFromPublicToCompany(BoxActionFromCompanyToPublicIncomeDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var publicMoenyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.IncomeId == dto.PublicIncomeId).First().Id;
                var boxAction = new BoxAction()
                {
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    IsIncmoe = true,
                    BoxActionType = BoxActionType.ReceiveFromPublicToCompany,
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    Date = DateTime.Now,
                    PubLicMoneyId = publicMoenyId
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var companyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == dto.CompanyId && c.CoinId == dto.CoinId).Single();
                companyCash.Total -= dto.Amount;
                _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);

                var companyCashFlow = new CompanyCashFlow()
                {
                    CoinId = dto.CoinId,
                    Amount = -dto.Amount,
                    CompanyId = dto.CompanyId,
                    Total = companyCash.Total,
                    MoneyActionId = moneyAction.Id
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                Tracing.SaveException(ex);
                return false;
            }
        }

        public BoxActionInitialDto InitialInputData()
        {
            var boxActionInitialDto = new BoxActionInitialDto();
            try
            {
                var currentTreasuryId = _appSession.GetCurrentTreasuryId();
                var treasuryCashes = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(x => x.TreasuryId == currentTreasuryId).ToList();
                var coins = treasuryCashes.Select(x => new CoinForDropdownDto() { Name = x.Coin.Name, Id = x.Coin.Id }).ToList();

                var agents = _unitOfWork.GenericRepository<Client>().FindBy(x => x.ClientType == ClientType.Client)
                    .Select(x => new ClientDto() { Id = x.Id, FullName = x.FullName, IsEnabled = x.IsEnabled }).ToList();

                var companies = _unitOfWork.GenericRepository<Company>().GetAll()
                    .Select(x => new CompanyForDropdownDto() { Id = x.Id, Name = x.Name }).ToList();

                var publicExpenses = _unitOfWork.GenericRepository<PublicExpense>().GetAll()
                   .Select(x => new PublicExpenseForDropdownDto() { Id = x.Id, Name = x.Name }).ToList();

                var publicIncomes = _unitOfWork.GenericRepository<PublicIncome>().GetAll()
                   .Select(x => new PublicIncomeForDropdownDto() { Id = x.Id, Name = x.Name }).ToList();

                boxActionInitialDto = new BoxActionInitialDto()
                {
                    Agents = agents,
                    TreasuryId = currentTreasuryId,
                    Coins = coins,
                    Companies = companies,
                    PublicExpenses = publicExpenses,
                    PublicIncomes = publicIncomes
                };
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return boxActionInitialDto;
        }
        #endregion

        #region Edit
        public bool EditExpenseFromTreasury(BoxActionExpensiveUpdateDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var publicActionMoneyId = _unitOfWork.GenericRepository<PublicMoney>()
                    .FindBy(c => c.ExpenseId == input.ExpensiveId).FirstOrDefault().Id;
                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();

                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == input.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    CoinId = input.CoinId,
                    IsIncmoe = false,
                    Note = input.Note,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);

                var moneyAction = new MoneyAction()
                {
                    PubLicMoneyId = publicActionMoneyId,
                    BoxAction = boxAction,
                    CreatedBy = _appSession.GetUserName(),
                    Date = date
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Amount = -input.Amount,
                    MoenyAction = moneyAction,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);

                var treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    Amount = -input.Amount,
                    CoinId = input.CoinId,
                    TreasuryId = treasuryId,
                    BranchCashFlow = branchCashFlow,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                var mainTreasuryId = _appSession.GetMainTreasury();
                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = -input.Amount,
                        BranchCashFlow = branchCashFlow,
                        CoinId = input.CoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
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

        public bool EditReceiveToTreasury(BoxActionIncomeUpdateDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var publicActionMoneyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.IncomeId == input.IncomeId).FirstOrDefault().Id;

                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == input.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    CoinId = input.CoinId,
                    IsIncmoe = true,
                    Note = input.Note,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);

                var moneyAction = new MoneyAction()
                {
                    PubLicMoneyId = publicActionMoneyId,
                    BoxAction = boxAction,
                    CreatedBy = _appSession.GetUserName(),
                    Date = date
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);

                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Amount = input.Amount,
                    MoenyAction = moneyAction,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);

                TreasuryMoneyAction treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    TreasuryId = treasuryId,
                    Amount = input.Amount,
                    BranchCashFlow = branchCashFlow,
                    CoinId = input.CoinId,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                var mainTreasuryId = _appSession.GetMainTreasury();
                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = input.Amount,
                        BranchCashFlow = branchCashFlow,
                        CoinId = input.CoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
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

        public bool EditExpenseFromTreasuryToClient(BoxActionClientUpdateDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == input.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    IsIncmoe = false,
                    CoinId = input.CoinId,
                    Note = input.Note,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
                    CreatedBy = _appSession.GetUserName(),
                    Date = date
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);

                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Amount = -input.Amount,
                    MoenyAction = moneyAction,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);
                TreasuryMoneyAction treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    TreasuryId = treasuryId,
                    CoinId = input.CoinId,
                    Amount = -input.Amount,
                    BranchCashFlow = branchCashFlow,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                var clientCash = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == input.ClientId && c.CoinId == input.CoinId).FirstOrDefault();
                clientCash.Total -= input.Amount;
                clientCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);

                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = input.CoinId,
                    ClientId = input.ClientId,
                    Total = clientCash.Total,
                    Amount = -input.Amount,
                    MoenyAction = moneyAction,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);

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

        public bool EditReceiveFromClientToTreasury(BoxActionClientUpdateDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();

                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == input.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    IsIncmoe = true,
                    CoinId = input.CoinId,
                    Note = input.Note,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
                    Date = date,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);

                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Amount = input.Amount,
                    MonyActionId = moneyAction.Id,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);
                TreasuryMoneyAction treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    TreasuryId = treasuryId,
                    BranchCashFlow = branchCashFlow,
                    Amount = input.Amount,
                    CoinId = input.CoinId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                var mainTreasuryId = _appSession.GetMainTreasury();
                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = input.Amount,
                        BranchCashFlow = branchCashFlow,
                        CoinId = input.CoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
                }

                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = input.CoinId,
                    ClientId = input.ClientId,
                    Amount = input.Amount,
                    MoenyAction = moneyAction,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);

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

        public bool EditReceiveFromCompanyToTreasury(BoxActionCompanyUpdateDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == input.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    IsIncmoe = true,
                    CoinId = input.CoinId,
                    Note = input.Note,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
                    Date = date,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);

                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Amount = input.Amount,
                    MonyActionId = moneyAction.Id,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);

                TreasuryMoneyAction treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    TreasuryId = treasuryId,
                    CoinId = input.CoinId,
                    Amount = input.Amount,
                    BranchCashFlow = branchCashFlow,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                var mainTreasuryId = _appSession.GetMainTreasury();
                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = input.Amount,
                        BranchCashFlow = branchCashFlow,
                        CoinId = input.CoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
                }

                var companyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == input.CompanyId && c.CoinId == input.CoinId).FirstOrDefault();
                companyCash.Total += input.Amount;
                _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);

                var companyCashFlow = new CompanyCashFlow()
                {
                    CoinId = input.CoinId,
                    CompanyId = input.CompanyId,
                    Amount = input.Amount,
                    Total = companyCash.Total,
                    MoenyAction = moneyAction,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);

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

        public bool EditExpenseFromTreasuryToCompany(BoxActionCompanyUpdateDto input)
        {
            try
            {
                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == input.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                var boxAction = new BoxAction()
                {
                    Amount = input.Amount,
                    IsIncmoe = false,
                    CoinId = input.CoinId,
                    Note = input.Note,
                    BoxActionType = BoxActionType.ExpenseFromTreasuryToCompany,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
                    Date = date,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                
                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = input.CoinId,
                    Amount = -input.Amount,
                    MoenyAction = moneyAction,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);
                TreasuryMoneyAction treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    TreasuryId = treasuryId,
                    Amount = -input.Amount,
                    BranchCashFlow = branchCashFlow,
                    CoinId = input.CoinId,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                var mainTreasuryId = _appSession.GetMainTreasury();
                if (mainTreasuryId != treasuryId)
                {
                    var mainTruseryMoneyAction = new TreasuryMoneyAction()
                    {
                        TreasuryId = mainTreasuryId,
                        Amount = -input.Amount,
                        BranchCashFlow = branchCashFlow,
                        CoinId = input.CoinId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(mainTruseryMoneyAction);
                }

                var companyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId == input.CompanyId && c.CoinId == input.CoinId).FirstOrDefault();
                companyCash.Total -= input.Amount;
                companyCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);

                var companyCashFlow = new CompanyCashFlow()
                {
                    CoinId = input.CoinId,
                    Amount = -input.Amount,
                    CompanyId = input.CompanyId,
                    Total = companyCash.Total,
                    MoenyAction = moneyAction,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);

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

        public bool EditFromClientToClient(BoxActionFromClientToClientUpdateDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == dto.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                Clearing clearing = new Clearing()
                {
                    FromClientId = dto.FirstClientId,
                    ToClientId = dto.SecondClientId,
                    IsIncome = dto.Amount > 0,
                    CoinId = dto.CoinId,
                    Note = dto.Note
                };
                _unitOfWork.GenericRepository<Clearing>().Insert(clearing);
                var moenyAction = new MoneyAction()
                {
                    ClearingId = clearing.Id,
                    Date = date,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moenyAction);
                ClientCashFlow firstClientCashFlow = new ClientCashFlow()
                {
                    ClientId = dto.FirstClientId,
                    CoinId = dto.CoinId,
                    MoenyActionId = moenyAction.Id,
                    Amount = dto.Amount,
                    
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(firstClientCashFlow);

                ClientCashFlow SecoundClientCashFlow = new ClientCashFlow()
                {
                    ClientId = dto.SecondClientId,
                    CoinId = dto.CoinId,
                    MoenyActionId = moenyAction.Id,
                    Amount = -dto.Amount,
                    
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(SecoundClientCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        public bool EditFromCompanyToClient(BoxActionFromCompanyToClientUpdateDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == dto.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                Clearing clearing = new Clearing()
                {
                    FromCompanyId = dto.CompanyId,
                    ToClientId = dto.ClientId,
                    IsIncome = dto.Amount > 0,
                    CoinId = dto.CoinId,
                    Note = dto.Note
                };
                _unitOfWork.GenericRepository<Clearing>().Insert(clearing);
                var moenyAction = new MoneyAction()
                {
                    Date = date,
                    ClearingId = clearing.Id
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moenyAction);
                CompanyCashFlow companyCashFlow = new CompanyCashFlow()
                {
                    CompanyId = dto.CompanyId,
                    CoinId = dto.CoinId,
                    MoneyActionId = moenyAction.Id,
                    Amount = dto.Amount,
                    
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);

                ClientCashFlow clientCashFlow = new ClientCashFlow()
                {
                    ClientId = dto.ClientId,
                    CoinId = dto.CoinId,
                    MoenyActionId = moenyAction.Id,
                    Amount = -dto.Amount
                    
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        public bool EditFromClientToCompany(BoxActionFromClientToCompanyUpdateDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == dto.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                Clearing clearing = new Clearing()
                {
                    FromClientId = dto.ClientId,
                    ToCompanyId = dto.CompanyId,
                    IsIncome = dto.Amount > 0,
                    CoinId = dto.CoinId,
                    Note = dto.Note
                };
                _unitOfWork.GenericRepository<Clearing>().Insert(clearing);
                var moenyAction = new MoneyAction()
                {
                    Date = date,
                    ClearingId = clearing.Id
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moenyAction);
                ClientCashFlow clientCashFlow = new ClientCashFlow()
                {
                    ClientId = dto.ClientId,
                    CoinId = dto.CoinId,
                    MoenyActionId = moenyAction.Id,
                    Amount = dto.Amount,
                    
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);

                CompanyCashFlow companyCashFlow = new CompanyCashFlow()
                {
                    CompanyId = dto.CompanyId,
                    CoinId = dto.CoinId,
                    MoneyActionId = moenyAction.Id,
                    Amount = -dto.Amount,
                    
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        public bool EditFromCompanyToCompany(BoxActionFromCompanyToCompanyUpdateDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == dto.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);
                Clearing clearing = new Clearing()
                {
                    FromCompanyId = dto.FirstCompanyId,
                    ToCompanyId = dto.SecondCompanyId,
                    IsIncome = dto.Amount > 0,
                    CoinId = dto.CoinId,
                    Note = dto.Note
                };
                _unitOfWork.GenericRepository<Clearing>().Insert(clearing);
                var moenyAction = new MoneyAction()
                {
                    ClearingId = clearing.Id,
                    Date = date
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moenyAction);
                CompanyCashFlow firstCompanyCahsFlwo = new CompanyCashFlow()
                {
                    CompanyId = dto.FirstCompanyId,
                    CoinId = dto.CoinId,
                    MoneyActionId = moenyAction.Id,
                    Amount = dto.Amount,
                    
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(firstCompanyCahsFlwo);

                CompanyCashFlow SecoundCompanyCashFlow = new CompanyCashFlow()
                {
                    CompanyId = dto.SecondCompanyId,
                    CoinId = dto.CoinId,
                    MoneyActionId = moenyAction.Id,
                    Amount = -dto.Amount,
                    
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(SecoundCompanyCashFlow);
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

        public bool EditExpenseFromClientToPublic(BoxActionFromClientToPublicExpenesUpdateDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == dto.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                var publicMoenyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.ExpenseId == dto.PublicExpenseId).First().Id;
                var boxAction = new BoxAction()
                {
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    IsIncmoe = false,
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    PubLicMoneyId = publicMoenyId,
                    Date = date
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                
                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    ClientId = dto.ClientId,
                    MoenyActionId = moneyAction.Id,
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                Tracing.SaveException(ex);
                return false;
            }
        }

        public bool EditReceiveFromPublicToClient(BoxActionFromClientToPublicIncomeUpdateDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == dto.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                var publicMoenyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.IncomeId == dto.PublicIncomeId).First().Id;
                var boxAction = new BoxAction()
                {
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    IsIncmoe = true,
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    Date = date,
                    PubLicMoneyId = publicMoenyId
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                
                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = dto.CoinId,
                    Amount = -dto.Amount,
                    ClientId = dto.ClientId,
                    MoenyActionId = moneyAction.Id,
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                Tracing.SaveException(ex);
                return false;
            }
        }

        public bool EditExpenseFromCompanyToPublic(BoxActionFromCompanyToPublicExpenesUpdateDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == dto.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                var publicMoenyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.ExpenseId == dto.PublicExpenseId).First().Id;
                var boxAction = new BoxAction()
                {
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    IsIncmoe = false,
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    Date = date,
                    PubLicMoneyId = publicMoenyId
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                
                var companyCashFlow = new CompanyCashFlow()
                {
                    CoinId = dto.CoinId,
                    Amount = +dto.Amount,
                    CompanyId = dto.CompanyId,
                    
                    MoneyActionId = moneyAction.Id,
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                Tracing.SaveException(ex);
                return false;
            }
        }

        public bool EditReceiveFromPublicToCompany(BoxActionFromCompanyToPublicIncomeUpdateDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.Id == dto.MoneyActionId).FirstOrDefault();

                var date = oldMoneyAction.Date;

                DeleteMoneyAction(oldMoneyAction);

                var publicMoenyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.IncomeId == dto.PublicIncomeId).First().Id;
                var boxAction = new BoxAction()
                {
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    IsIncmoe = true,
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    Date = date,
                    PubLicMoneyId = publicMoenyId
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                

                var companyCashFlow = new CompanyCashFlow()
                {
                    CoinId = dto.CoinId,
                    Amount = -dto.Amount,
                    CompanyId = dto.CompanyId,
                    MoneyActionId = moneyAction.Id
                };
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                Tracing.SaveException(ex);
                return false;
            }
        }
        #endregion

        #region Help Methods
        public void DeleteMoneyAction(MoneyAction moneyAction)
        {
            

            //Old Company Cash Flows
            var oldCompanyCashFlows = moneyAction.CompanyCashFlows.ToList();
            foreach (var oldCompanyCashFlow in oldCompanyCashFlows)
            {
                _unitOfWork.GenericRepository<CompanyCashFlow>().Delete(oldCompanyCashFlow);
            }

            //Old Client Cash Flows
            var oldClientCashFlows = moneyAction.ClientCashFlows.ToList();
            foreach (var oldClientCashFlow in oldClientCashFlows)
            {
                _unitOfWork.GenericRepository<ClientCashFlow>().Delete(oldClientCashFlow);
            }

            //Old Branch Cash Flows
            var oldBranchCashFlows = moneyAction.BranchCashFlows.ToList();
            var oldBranchCashFlowsIds = oldBranchCashFlows.Select(b => b.Id).ToList();
            if (oldBranchCashFlows.Any())
            {
                var oldTreasuryMoneyActions = _unitOfWork.GenericRepository<TreasuryMoneyAction>()
                .FindBy(x => oldBranchCashFlowsIds.Contains((int)x.BranchCashFlowId)).ToList();

                foreach (var oldTreasuryMoneyAction in oldTreasuryMoneyActions)
                {
                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Delete(oldTreasuryMoneyAction);
                }

                foreach (var oldBranchCashFlow in oldBranchCashFlows)
                {
                    _unitOfWork.GenericRepository<BranchCashFlow>().Delete(oldBranchCashFlow);
                }
            }

            var oldBoxAction = moneyAction.BoxAction;
            if(oldBoxAction != null)
                _unitOfWork.GenericRepository<BoxAction>().Delete(oldBoxAction);

            var oldClearing = moneyAction.Clearing;
            if(oldClearing != null)
                _unitOfWork.GenericRepository<Clearing>().Delete(oldClearing);

            _unitOfWork.GenericRepository<MoneyAction>().Delete(moneyAction);
        }


        #endregion
    }
}
