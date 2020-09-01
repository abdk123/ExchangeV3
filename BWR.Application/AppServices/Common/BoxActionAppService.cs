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

        public bool PayExpenciveFromMainBox(BoxActionExpensiveDto input)
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

        public bool ReciverIncomeToMainBox(BoxActionIncomeDto input)
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

        public bool PayForClientFromMainBox(BoxActionClientDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();


                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();

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
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
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

        public bool ReciveFromClientToMainBox(BoxActionClientDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();

                var treuseryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(c => c.CoinId == input.CoinId && c.TreasuryId == treasuryId).FirstOrDefault();
                treuseryCash.Total += input.Amount;
                treuseryCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treuseryCash);
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treuseryCash);

                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.BranchId == branchId && c.CoinId == input.CoinId).FirstOrDefault();
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

        public bool ReciveFromCompanyToMainBox(BoxActionCompanyDto input)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();

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
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
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

        public bool PayForCompanyFromMainBox(BoxActionCompanyDto input)
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
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxAction = boxAction,
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
        public bool FromClientToPublicExpenes(BoxActionFromClientToPublicExpenesDto dto)
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
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    PubLicMoneyId = publicMoenyId
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var clientCash= _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == dto.ClientId && c.CoinId == dto.CoinId).Single();
                clientCash.Total -= dto.Amount;
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);
                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = dto.CoinId,
                    Amount=  -dto.Amount,
                    ClientId = dto.ClientId,
                    Total = clientCash.Total,
                    MoenyActionId= moneyAction.Id,
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch(Exception ex)
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

        public bool FromClientToPublicIncome(BoxActionFromClientToPublicIncomeDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var publicMoenyId = _unitOfWork.GenericRepository<PublicMoney>().FindBy(c => c.IncomeId== dto.PublicIncomeId).First().Id;
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

        public bool FromCompanyToPublicExpenes(BoxActionFromCompanyToPublicExpenesDto dto)
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
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    PubLicMoneyId = publicMoenyId
                };
                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                var companyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CompanyId== dto.CompanyId && c.CoinId == dto.CoinId).Single();
                companyCash.Total -= dto.Amount;
                _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);
                var companyCashFlow = new CompanyCashFlow()
                {
                    CoinId = dto.CoinId,
                    Amount = -dto.Amount,
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

        public bool FromCompanyToPublicIncome(BoxActionFromCompanyToPublicIncomeDto dto)
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
                    Note = dto.Note,
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
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
    }
}
