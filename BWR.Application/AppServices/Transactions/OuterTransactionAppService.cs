using AutoMapper;
using BWR.Application.Dtos.Client;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Dtos.Setting.Country;
using BWR.Application.Dtos.Transaction.OuterTransaction;
using BWR.Application.Interfaces.Shared;
using BWR.Application.Interfaces.Transaction;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using BWR.Domain.Model.Treasures;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System.Collections.Generic;
using System.Linq;
using BWR.Application.Dtos.Company;
using BWR.Application.Dtos.Setting.Attachment;
using BWR.Domain.Model.Companies;
using BWR.Application.Dtos.Branch;
using BWR.Infrastructure.Exceptions;
using BWR.Domain.Model.Common;
using System;
using BWR.Domain.Model.Branches;
using BWR.Application.Dtos.MoneyAction;

namespace BWR.Application.AppServices.Transactions
{
    public class OuterTransactionAppService : IOuterTransactionAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public OuterTransactionAppService(
            IUnitOfWork<MainContext> unitOfWork,
            IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        #region Get
        public IList<OuterTransactionDto> GetTransactions(OuterTransactionInputDto input)
        {
            IList<OuterTransactionDto> outerTransactionsDto = new List<OuterTransactionDto>();

            try
            {
                var outerTransactions = _unitOfWork.GenericRepository<Transaction>().FindBy(x => x.TransactionType == TransactionType.ExportTransaction);

                if (input.CoinId != null)
                {
                    outerTransactions = outerTransactions.Where(x => x.CoinId == input.CoinId);
                }

                if (input.CountryId != null)
                {
                    outerTransactions = outerTransactions.Where(x => x.CountryId == input.CountryId);
                }

                if (input.ReceiverClientId != null)
                {
                    outerTransactions = outerTransactions.Where(x => x.ReciverClientId == input.ReceiverClientId);
                }

                if (input.SenderClientId != null)
                {
                    outerTransactions = outerTransactions.Where(x => x.SenderClientId == input.SenderClientId);
                }

                if (input.From != null)
                {
                    outerTransactions = outerTransactions.Where(x => x.Created >= input.From);
                }
                if (input.To != null)
                {
                    outerTransactions = outerTransactions.Where(x => x.Created <= input.To);
                }

                outerTransactionsDto = Mapper.Map<List<Transaction>, List<OuterTransactionDto>>(outerTransactions.ToList());
            }
            catch (Exception ex)
            {
                Infrastructure.Exceptions.Tracing.SaveException(ex);
            }
            return outerTransactionsDto;
        }

        public OuterTransactionDto GetTransactionById(int id)
        {
            OuterTransactionDto outerTransactionDto = null;

            try
            {
                var outerTransaction = _unitOfWork.GenericRepository<Transaction>().GetById(id);

                if (outerTransaction != null && outerTransaction.TransactionType != TransactionType.ImportTransaction)
                {
                    outerTransactionDto = Mapper.Map<Transaction, OuterTransactionDto>(outerTransaction);
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Exceptions.Tracing.SaveException(ex);
            }
            return outerTransactionDto;
        }

        public OuterTransactionUpdateDto GetOuterTransactionForEdit(int id)
        {
            OuterTransactionUpdateDto outerTransactionUpdateDto = null;

            try
            {
                var outerTransaction = _unitOfWork.GenericRepository<Transaction>().GetById(id);

                outerTransactionUpdateDto = Mapper.Map<Transaction, OuterTransactionUpdateDto>(outerTransaction);
            }
            catch (Exception ex)
            {
                Infrastructure.Exceptions.Tracing.SaveException(ex);
            }
            return outerTransactionUpdateDto;
        }

        public OuterTransactionInsertInitialDto InitialInputData()
        {
            var currentTreasuryId = _appSession.GetCurrentTreasuryId();
            var treasuryCashes = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(x => x.TreasuryId == currentTreasuryId).ToList();
            var coins = treasuryCashes.Select(x => new CoinForDropdownDto() { Name = x.Coin.Name, Id = x.Coin.Id }).ToList();

            var countries = _unitOfWork.GenericRepository<Country>().FindBy(x => x.IsEnabled == true)
                .Select(x => new CountryForDropdownDto() { Id = x.Id, Name = x.Name }).ToList();

            var clients = _unitOfWork.GenericRepository<Client>().FindBy(x => x.ClientType == ClientType.Normal)
                .Select(x => new ClientDto() { Id = x.Id, FullName = x.FullName, IsEnabled = x.IsEnabled }).ToList();

            var agents = _unitOfWork.GenericRepository<Client>().FindBy(x => x.ClientType == ClientType.Client)
                .Select(x => new ClientDto() { Id = x.Id, FullName = x.FullName, IsEnabled = x.IsEnabled }).ToList();

            var companies = _unitOfWork.GenericRepository<Company>().GetAll()
                .Select(x => new CompanyForDropdownDto() { Id = x.Id, Name = x.Name }).ToList();

            var attachments = _unitOfWork.GenericRepository<Attachment>().GetAll()
                .Select(x => new AttachmentForDropdownDto() { Id = x.Id, Name = x.Name }).ToList();

            var outerTransactionInsertInputDto = new OuterTransactionInsertInitialDto()
            {
                Coins = coins,
                Countries = countries,
                Agents = agents,
                Clients = clients,
                TreasuryId = currentTreasuryId,
                Companies = companies,
                Attachments = attachments
            };

            return outerTransactionInsertInputDto;
        }
        #endregion

        #region Insert

        public bool OuterClientTransaction(OuterTransactionInsertDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var outerTransaction = Mapper.Map<OuterTransactionInsertDto, Transaction>(dto);
                outerTransaction.ReceiverBranchId = BranchHelper.Id;
                outerTransaction.TreaseryId = treasuryId;
                outerTransaction.TransactionsStatus = TransactionStatus.None;
                outerTransaction.TransactionType = TransactionType.ExportTransaction;
                outerTransaction.TypeOfPay = TypeOfPay.Cash;
                outerTransaction.CreatedBy = _appSession.GetUserName();
                //outerTransaction.SenderClientId = dto.SenderClientId;
                //outerTransaction.ReciverClientId = dto.ReciverClientId;
                _unitOfWork.GenericRepository<Transaction>().Insert(outerTransaction);

                _unitOfWork.Save();

                #region Money Action
                var moneyAction = new MoneyAction()
                {
                    //TransactionId = outerTransaction.Id,
                    Transaction = outerTransaction,
                    CreatedBy = _appSession.GetUserName(),
                    Date = dto.Date
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                _unitOfWork.Save();
                #endregion

                #region Branch Cash
                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(x => x.BranchId == BranchHelper.Id && x.CoinId == dto.CoinId).FirstOrDefault();
                branchCash.Total += dto.Amount + dto.OurComission;
                branchCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);

                #endregion

                #region Branch Cash Flow
                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = BranchHelper.Id,
                    CreatedBy = _appSession.GetUserName(),
                    MonyActionId = moneyAction.Id,
                    TreasuryId = treasuryId,
                    CoinId = dto.CoinId,
                    Amount = dto.Amount + dto.OurComission,
                    Total = branchCash.Total
                };

                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);
                #endregion

                #region Company Cash
                var companyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(x => x.CompanyId == dto.SenderCompanyId && x.CoinId == dto.CoinId).FirstOrDefault();
                if (companyCash != null)
                {
                    companyCash.Total += dto.Amount + dto.SenderCompanyComission.Value;
                    companyCash.ModifiedBy = _appSession.GetUserName();
                    _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);
                }

                #endregion

                #region Company Cash Flow
                var companyCashFlow = new CompanyCashFlow()
                {
                    CreatedBy = _appSession.GetUserName(),
                    CompanyId = dto.SenderCompanyId.Value,
                    MoneyActionId = moneyAction.Id,
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    Total = companyCash.Total,
                    Matched = false
                };

                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                _unitOfWork.Save();
                #endregion

                #region Treasury Cash
                var treasuryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(x => x.TreasuryId == treasuryId && x.CoinId == dto.CoinId).FirstOrDefault();
                if (treasuryCash != null)
                {
                    treasuryCash.Total += dto.Amount + dto.OurComission;
                }
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treasuryCash);

                #endregion

                #region Treasury Money Action
                var truseryMoneyAction = new TreasuryMoneyAction()
                {
                    Total = treasuryCash.Total,
                    Amount = dto.Amount + dto.OurComission,
                    TreasuryId = treasuryId,
                    CoinId = dto.CoinId,
                    BranchCashFlow = branchCashFlow,
                };

                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(truseryMoneyAction);

                #endregion

                _unitOfWork.Save();
                _unitOfWork.Commit();

                return true;

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
                return false;
            }

        }

        public bool OuterAgentTransaction(OuterTransactionInsertDto dto)
        {

            try
            {
                _unitOfWork.CreateTransaction();

                var treasuryId = _appSession.GetCurrentTreasuryId();
                var outerTransaction = Mapper.Map<OuterTransactionInsertDto, Transaction>(dto);
                outerTransaction.ReceiverBranchId = BranchHelper.Id;
                outerTransaction.TreaseryId = treasuryId;
                outerTransaction.TransactionsStatus = TransactionStatus.None;
                outerTransaction.TransactionType = TransactionType.ExportTransaction;
                outerTransaction.TypeOfPay = TypeOfPay.ClientsReceivables;
                outerTransaction.CreatedBy = _appSession.GetUserName();

                _unitOfWork.GenericRepository<Transaction>().Insert(outerTransaction);


                #region Money Action
                var moneyAction = new MoneyAction()
                {
                    Date = dto.Date,
                    Transaction = outerTransaction,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                #endregion

                #region Branch Cash
                //var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(x => x.BranchId == BranchHelper.Id && x.CoinId == dto.CoinId).FirstOrDefault();
                //if (branchCash != null)
                //{
                //    branchCash.Total += dto.Amount + dto.OurComission;
                //    branchCash.ModifiedBy = _appSession.GetUserName();
                //    _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);
                //}

                #endregion

                #region Branch Cash Flow
                //var branchCashFlow = new BranchCashFlow()
                //{
                //    BranchId = BranchHelper.Id,
                //    CreatedBy = _appSession.GetUserName(),
                //    MoenyAction = moneyAction,
                //    TreasuryId = treasuryId,
                //    CoinId = dto.CoinId,
                //    Amount = dto.Amount + dto.OurComission,
                //    Total = branchCash.Total
                //};

                //_unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);
                #endregion

                #region Company Cash
                var companyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(x => x.CompanyId == dto.SenderCompanyId && x.CoinId == dto.CoinId).FirstOrDefault();
                if (companyCash != null)
                {
                    companyCash.Total += dto.Amount + dto.SenderCompanyComission.Value;
                    companyCash.ModifiedBy = _appSession.GetUserName();
                    _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);
                }

                #endregion

                #region Company Cash Flow
                var companyCashFlow = new CompanyCashFlow()
                {
                    CreatedBy = _appSession.GetUserName(),
                    CompanyId = dto.SenderCompanyId.Value,
                    MoenyAction = moneyAction,
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    Total = companyCash.Total,
                    Matched = false
                };

                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                #endregion

                #region Client Cash
                var clientCash = _unitOfWork.GenericRepository<ClientCash>().FindBy(x => x.ClientId == dto.SenderClientId && x.CoinId == dto.CoinId).FirstOrDefault();
                if (clientCash != null)
                {
                    clientCash.Total -= dto.Amount + dto.OurComission;
                    if (dto.SenderCleirntCommission != null && dto.SenderCleirntCommission != 0)
                        clientCash.Total += (decimal)dto.SenderCleirntCommission;
                    clientCash.ModifiedBy = _appSession.GetUserName();
                }
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);

                #endregion

                #region Client Cash Flow
                var clientCashFlow = new ClientCashFlow()
                {
                    ClientId = dto.SenderClientId.Value,
                    Total = clientCash.Total,
                    MoenyAction = moneyAction,
                    CoinId = dto.CoinId,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName(),
                    Amount = -dto.Amount
                };
                //if (dto.SenderCleirntCommission != null && dto.SenderCleirntCommission != 0)
                //{
                //    clientCashFlow.Amount -= dto.SenderCleirntCommission.Value;
                //}
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);

                #endregion

                _unitOfWork.Save();
                _unitOfWork.Commit();

                if (dto.RecivingAmount != null && dto.RecivingAmount != 0)
                {
                    bool response = ReciveFromClientForMainBoxMethond(outerTransaction.Id, dto);
                    if (!response)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
                return false;
            }
        }

        public bool OuterCompanyTranasction(OuterTransactionInsertDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var outerTransaction = Mapper.Map<OuterTransactionInsertDto, Transaction>(dto);
                outerTransaction.ReceiverBranchId = BranchHelper.Id;
                outerTransaction.TreaseryId = treasuryId;
                outerTransaction.TransactionsStatus = TransactionStatus.None;
                outerTransaction.TransactionType = TransactionType.ExportTransaction;
                outerTransaction.TypeOfPay = TypeOfPay.CompaniesReceivables;
                outerTransaction.CreatedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<Transaction>().Insert(outerTransaction);

                #region Money Action
                var moneyAction = new MoneyAction()
                {
                    //TransactionId = outerTransaction.Id,
                    Transaction = outerTransaction,
                    CreatedBy = _appSession.GetUserName(),
                    Date = dto.Date
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                #endregion

                #region Sender Company Cash
                var senderCompanyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(x => x.CompanyId == dto.SenderCompanyId && x.CoinId == dto.CoinId).FirstOrDefault();
                if (senderCompanyCash != null)
                {
                    senderCompanyCash.Total += dto.Amount + dto.SenderCompanyComission.Value;
                    senderCompanyCash.ModifiedBy = _appSession.GetUserName();
                    _unitOfWork.GenericRepository<CompanyCash>().Update(senderCompanyCash);
                }

                #endregion

                #region Sender Company Cash Flow
                var senderCompanyCashFlow = new CompanyCashFlow()
                {
                    CreatedBy = _appSession.GetUserName(),
                    CompanyId = dto.SenderCompanyId.Value,
                    MoenyAction = moneyAction,
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    Total = senderCompanyCash.Total,
                    Matched = false
                };

                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(senderCompanyCashFlow);
                #endregion

                #region Receiver Company Cash
                var receiverCompanyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(x => x.CompanyId == dto.ReceiverCompanyId && x.CoinId == dto.CoinId).FirstOrDefault();
                if (receiverCompanyCash != null)
                {
                    receiverCompanyCash.Total -= dto.Amount;
                    receiverCompanyCash.Total -= dto.OurComission;
                    receiverCompanyCash.Total += dto.ReceiverCompanyComission.Value;
                    receiverCompanyCash.ModifiedBy = _appSession.GetUserName();
                    _unitOfWork.GenericRepository<CompanyCash>().Update(receiverCompanyCash);
                }

                #endregion

                #region Receiver Company Cash Flow
                var receiverCompanyCashFlow = new CompanyCashFlow()
                {
                    CreatedBy = _appSession.GetUserName(),
                    CompanyId = dto.ReceiverCompanyId.Value,
                    MoenyAction = moneyAction,
                    CoinId = dto.CoinId,
                    Amount = (dto.Amount + dto.OurComission) * -1,
                    Total = receiverCompanyCash.Total,
                    Matched = false
                };

                receiverCompanyCashFlow.Amount += dto.ReceiverCompanyComission.Value;
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(receiverCompanyCashFlow);
                #endregion

                _unitOfWork.Save();
                _unitOfWork.Commit();

                //if (dto.RecivingAmount != null && dto.RecivingAmount != 0)
                //{
                //    bool response = ReciveFromClientForMainBoxMethond(outerTransaction.Id, dto);
                //    if (!response)
                //    {
                //        return false;
                //    }
                //}

                return true;

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
                return false;
            }

        }

        #endregion

        #region Edit

        public bool EditOuterTransactionForClient(OuterTransactionUpdateDto dto)
        {
            try
            {
                var treasuryId = _appSession.GetCurrentTreasuryId();
                _unitOfWork.CreateTransaction();

                var outerTransaction = _unitOfWork.GenericRepository<Transaction>().GetById(dto.Id);
                if (outerTransaction == null)
                {
                    _unitOfWork.Rollback();
                    return false;
                }

                decimal oldAmount = outerTransaction.Amount;
                decimal oldSenderCompanyComission = outerTransaction.SenderCompanyComission != null ? outerTransaction.SenderCompanyComission.Value : 0;
                decimal oldOurCommission = outerTransaction.OurComission;
                int oldCoinId = outerTransaction.CoinId;
                int oldTreasuryId = outerTransaction.TreaseryId;
                var oldSenderCompanyId = outerTransaction.SenderCompanyId;
                var oldSenderClientId = outerTransaction.SenderClientId != null ? outerTransaction.SenderClientId.Value : 0;

                //outerTransaction.ReceiverBranchId = BranchHelper.Id;
                //outerTransaction.ReciverClientId = dto.ReciverClientId;
                //outerTransaction.TreaseryId = treasuryId;
                //outerTransaction.Amount = dto.Amount;
                //outerTransaction.ReciverClientId = dto.ReciverClientId;
                //outerTransaction.SenderCompanyId = dto.SenderCompanyId;
                //outerTransaction.SenderCompanyComission = dto.SenderCompanyComission;
                //outerTransaction.OurComission = dto.OurComission;
                //outerTransaction.CountryId = dto.CountryId;
                //outerTransaction.Reason = dto.Reason;
                //outerTransaction.Note = dto.Note;

                Mapper.Map<OuterTransactionUpdateDto, Transaction>(dto, outerTransaction);

                outerTransaction.ReceiverBranchId = BranchHelper.Id;
                outerTransaction.TreaseryId = treasuryId;
                outerTransaction.TransactionsStatus = TransactionStatus.None;
                outerTransaction.TransactionType = TransactionType.ExportTransaction;
                outerTransaction.TypeOfPay = TypeOfPay.Cash;
                outerTransaction.ModifiedBy = _appSession.GetUserName();
                
                _unitOfWork.GenericRepository<Transaction>().Update(outerTransaction);

                #region Money Action
                var moneyActionDto = dto.MoenyActions.Where(x => x.BoxActionsId == null).FirstOrDefault();
                if (moneyActionDto == null)
                {
                    _unitOfWork.Rollback();
                    return false;
                }

                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().GetById(moneyActionDto.Id);
                moneyAction.ModifiedBy = _appSession.GetUserName();
                if (moneyActionDto.Date != null)
                {
                    moneyAction.Date = moneyActionDto.Date.Value;
                }

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

                _unitOfWork.GenericRepository<MoneyAction>().Update(moneyAction);

                var moneyActionContainBoxActionDto = dto.MoenyActions.Where(x => x.BoxActionsId != null).FirstOrDefault();
                if (moneyActionContainBoxActionDto != null)
                {
                    var moneyActionContainBoxAction = _unitOfWork.GenericRepository<MoneyAction>()
                        .FindBy(x => x.Id == moneyActionContainBoxActionDto.Id).FirstOrDefault();

                    DeleteIncomeBoxAction(moneyActionContainBoxAction);
                }
                #endregion

                #region Branch Cash Flow

                var newBranchCashFlow = new BranchCashFlow()
                {
                    BranchId = BranchHelper.Id,
                    CreatedBy = _appSession.GetUserName(),
                    MonyActionId = moneyAction.Id,
                    TreasuryId = treasuryId,
                    CoinId = dto.CoinId,
                    Amount = dto.Amount + dto.OurComission,
                };

                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(newBranchCashFlow);

                #endregion

                #region Company Cash Flow

                var newCompanyCashFlow = new CompanyCashFlow()
                {
                    CreatedBy = _appSession.GetUserName(),
                    CompanyId = dto.SenderCompanyId.Value,
                    MoneyActionId = moneyAction.Id,
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    Matched = false
                };

                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(newCompanyCashFlow);

                #endregion

                #region Treasury Money Action

                var newTruseryMoneyAction = new TreasuryMoneyAction()
                {
                    Amount = dto.Amount + dto.OurComission,
                    TreasuryId = treasuryId,
                    CoinId = dto.CoinId,
                    BranchCashFlow = newBranchCashFlow,
                };

                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(newTruseryMoneyAction);

                #endregion

                _unitOfWork.Save();
                _unitOfWork.Commit();

                return true;

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
                return false;
            }

        }

        public bool EditOuterTransactionForAgent(OuterTransactionUpdateDto dto)
        {

            try
            {
                _unitOfWork.CreateTransaction();

                var treasuryId = _appSession.GetCurrentTreasuryId();
                var outerTransaction = _unitOfWork.GenericRepository<Transaction>().GetById(dto.Id);
                if (outerTransaction == null)
                {
                    _unitOfWork.Rollback();
                    return false;
                }

                decimal oldAmount = outerTransaction.Amount;
                decimal oldSenderCompanyComission = outerTransaction.SenderCompanyComission != null ? outerTransaction.SenderCompanyComission.Value : 0;
                decimal oldSenderClientComission = outerTransaction.SenderCleirntCommission != null ? outerTransaction.SenderCleirntCommission.Value : 0;
                decimal oldOurCommission = outerTransaction.OurComission;
                decimal oldRecivingAmount = outerTransaction.RecivingAmount != null ? outerTransaction.RecivingAmount.Value : 0;
                int oldCoinId = outerTransaction.CoinId;
                int oldTreasuryId = outerTransaction.TreaseryId;
                var oldSenderCompanyId = outerTransaction.SenderCompanyId;
                var oldSenderClientId = outerTransaction.SenderClientId != null ? outerTransaction.SenderClientId.Value : 0;

                //outerTransaction.ReceiverBranchId = BranchHelper.Id;
                //outerTransaction.TreaseryId = treasuryId;
                //outerTransaction.Amount = dto.Amount;
                //outerTransaction.SenderCompanyComission = dto.SenderCompanyComission;
                //outerTransaction.SenderCleirntCommission = dto.SenderCleirntCommission;
                //outerTransaction.OurComission = dto.OurComission;
                //outerTransaction.Reason = dto.Reason;
                //outerTransaction.Note = dto.Note;
                //outerTransaction.RecivingAmount = dto.RecivingAmount;

                Mapper.Map<OuterTransactionUpdateDto, Transaction>(dto, outerTransaction);
                outerTransaction.ReceiverBranchId = BranchHelper.Id;
                outerTransaction.TreaseryId = treasuryId;
                outerTransaction.TransactionsStatus = TransactionStatus.None;
                outerTransaction.TransactionType = TransactionType.ExportTransaction;
                outerTransaction.TypeOfPay = TypeOfPay.ClientsReceivables;
                outerTransaction.ModifiedBy = _appSession.GetUserName();

                _unitOfWork.GenericRepository<Transaction>().Update(outerTransaction);
                

                #region Money Action

                var moneyActionDto = dto.MoenyActions.Where(x => x.BoxActionsId == null).FirstOrDefault();
                if (moneyActionDto == null)
                {
                    _unitOfWork.Rollback();
                    return false;
                }

                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().GetById(moneyActionDto.Id);
                if (moneyActionDto.Date != null && moneyAction != null)
                {
                    moneyAction.ModifiedBy = _appSession.GetUserName();
                    moneyAction.Date = moneyActionDto.Date.Value;

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

                    _unitOfWork.GenericRepository<MoneyAction>().Update(moneyAction);
                }

                #endregion

                #region Company Cash Flow
                
                var companyCashFlow = new CompanyCashFlow()
                {
                    CreatedBy = _appSession.GetUserName(),
                    CompanyId = dto.SenderCompanyId.Value,
                    MoenyAction = moneyAction,
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    Matched = false
                };

                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                #endregion

                #region Client Cash Flow

                var clientCashFlow = new ClientCashFlow()
                {
                    ClientId = dto.SenderClientId.Value,
                    MoenyAction = moneyAction,
                    CoinId = dto.CoinId,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName(),
                    Amount = -dto.Amount
                };
                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);

                #endregion

                if (dto.RecivingAmount != null && dto.RecivingAmount != 0)
                {
                    bool response = EditReciveFromClientForMainBoxMethond(dto,oldRecivingAmount,oldCoinId,oldTreasuryId,oldSenderClientId,oldSenderClientComission);
                    if (!response)
                    {
                        return false;
                    }
                }

                _unitOfWork.Save();
                _unitOfWork.Commit();

                return true;
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
                return false;
            }
        }

        public bool EditOuterTranasctionForCompany(OuterTransactionUpdateDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var treasuryId = _appSession.GetCurrentTreasuryId();
                var outerTransaction = _unitOfWork.GenericRepository<Transaction>().GetById(dto.Id);
                if (outerTransaction == null)
                {
                    _unitOfWork.Rollback();
                    return false;
                }

                decimal oldAmount = outerTransaction.Amount;
                decimal oldSenderCompanyComission = outerTransaction.SenderCompanyComission != null ? outerTransaction.SenderCompanyComission.Value : 0;
                decimal oldReceiverCompanyComission = outerTransaction.ReceiverCompanyComission != null ? outerTransaction.ReceiverCompanyComission.Value : 0;
                decimal oldOurCommission = outerTransaction.OurComission;
                decimal oldRecivingAmount = outerTransaction.RecivingAmount != null ? outerTransaction.RecivingAmount.Value : 0;
                int oldCoinId = outerTransaction.CoinId;
                int oldTreasuryId = outerTransaction.TreaseryId;
                var oldSenderCompanyId = outerTransaction.SenderCompanyId;
                var oldReceiverCompanyId = outerTransaction.ReceiverCompanyId != null ? outerTransaction.ReceiverCompanyId.Value : 0;
                var oldSenderClientId = outerTransaction.SenderClientId != null ? outerTransaction.SenderClientId.Value : 0;

                //outerTransaction.ReceiverBranchId = BranchHelper.Id;
                //outerTransaction.TreaseryId = treasuryId;
                //outerTransaction.Amount = dto.Amount;
                //outerTransaction.SenderCompanyComission = dto.SenderCompanyComission;
                //outerTransaction.SenderCleirntCommission = dto.SenderCleirntCommission;
                //outerTransaction.OurComission = dto.OurComission;
                //outerTransaction.Reason = dto.Reason;
                //outerTransaction.Note = dto.Note;

                Mapper.Map<OuterTransactionUpdateDto, Transaction>(dto, outerTransaction);

                outerTransaction.ReceiverBranchId = BranchHelper.Id;
                outerTransaction.TreaseryId = treasuryId;
                outerTransaction.TransactionsStatus = TransactionStatus.None;
                outerTransaction.TransactionType = TransactionType.ExportTransaction;
                outerTransaction.TypeOfPay = TypeOfPay.CompaniesReceivables;
                outerTransaction.ModifiedBy = _appSession.GetUserName();

                _unitOfWork.GenericRepository<Transaction>().Update(outerTransaction);

                #region Monet Action
                var moneyActionDto = dto.MoenyActions.Where(x => x.BoxActionsId == null).FirstOrDefault();
                if (moneyActionDto == null)
                {
                    _unitOfWork.Rollback();
                    return false;
                }

                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().GetById(moneyActionDto.Id);
                if (moneyActionDto.Date != null && moneyAction != null)
                {
                    moneyAction.ModifiedBy = _appSession.GetUserName();
                    moneyAction.Date = moneyActionDto.Date.Value;

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

                    _unitOfWork.GenericRepository<MoneyAction>().Update(moneyAction);

                    var moneyActionContainBoxActionDto = dto.MoenyActions.Where(x => x.BoxActionsId != null).FirstOrDefault();
                    if (moneyActionContainBoxActionDto != null)
                    {
                        var moneyActionContainBoxAction = _unitOfWork.GenericRepository<MoneyAction>()
                            .FindBy(x => x.Id == moneyActionContainBoxActionDto.Id).FirstOrDefault();

                        DeleteIncomeBoxAction(moneyActionContainBoxAction);
                    }
                }

                #endregion

                #region Sender Company Cash Flow
                
                var companyCashFlow = new CompanyCashFlow()
                {
                    CreatedBy = _appSession.GetUserName(),
                    CompanyId = dto.SenderCompanyId.Value,
                    MoenyAction = moneyAction,
                    CoinId = dto.CoinId,
                    Amount = dto.Amount,
                    Matched = false
                };

                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                #endregion

                #region Receiver Company Cash Flow
                
                var receiverCompanyCashFlow = new CompanyCashFlow()
                {
                    CreatedBy = _appSession.GetUserName(),
                    CompanyId = dto.ReceiverCompanyId.Value,
                    MoenyAction = moneyAction,
                    CoinId = dto.CoinId,
                    Amount = (dto.Amount + dto.OurComission) * -1,
                    Matched = false
                };

                receiverCompanyCashFlow.Amount += dto.ReceiverCompanyComission.Value;
                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(receiverCompanyCashFlow);
                #endregion


                _unitOfWork.Save();
                _unitOfWork.Commit();

                //if (dto.RecivingAmount != null && dto.RecivingAmount != 0)
                //{
                //    bool response = EditReciveFromCompanyForMainBoxMethond(dto, oldRecivingAmount, oldRecivingAmount, oldCoinId, oldTreasuryId, oldSenderCompanyId.Value,oldSenderCompanyComission);
                //    if (!response)
                //    {
                //        return false;
                //    }
                //}

                return true;

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
                return false;
            }

        }


        #endregion

        #region Other
        public bool ReciveFromClientForMainBoxMethond(int transactionId, OuterTransactionInsertDto dto)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.BranchId == branchId && c.CoinId == dto.CoinId).FirstOrDefault();
                var boxAction = new BoxAction()
                {
                    Amount = (decimal)dto.RecivingAmount,
                    IsIncmoe = true,
                    CoinId = dto.CoinId,
                    Note = dto.Note,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    Date = dto.Date,
                    BoxActionsId = boxAction.Id,
                    TransactionId = transactionId,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                branchCash.Total += (decimal)dto.RecivingAmount;
                branchCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);

                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = dto.CoinId,
                    Total = branchCash.Total,
                    Amount = (decimal)dto.RecivingAmount,
                    MonyActionId = moneyAction.Id,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);

                var treuseryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(c => c.CoinId == dto.CoinId && c.TreasuryId == treasuryId).FirstOrDefault();
                treuseryCash.Total += (decimal)dto.RecivingAmount;
                treuseryCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treuseryCash);
                var treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    BranchCashFlowId = branchCashFlow.Id,
                    CoinId = dto.CoinId,
                    Total = treuseryCash.Total,
                    TreasuryId = treasuryId,
                    Amount = (decimal)dto.RecivingAmount,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                var clientCash = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == dto.SenderClientId && c.CoinId == dto.CoinId).First();
                clientCash.Total += (decimal)dto.RecivingAmount;
                clientCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);


                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = dto.CoinId,
                    ClientId = (int)dto.SenderClientId,
                    Total = clientCash.Total,
                    Amount = (decimal)dto.RecivingAmount,
                    MoenyActionId = moneyAction.Id,
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

        public bool EditReciveFromClientForMainBoxMethond(OuterTransactionUpdateDto dto, decimal oldRecivingAmount, int oldCoinId, int oldTreasuryId, int oldSenderClientId, decimal oldSenderClientCommission)
        {
            try
            {
                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();

                MoneyAction moneyAction = null;
                var moneyActionDetailDto = dto.MoenyActions.FirstOrDefault(x => x.BoxActionsId != null);
                if (moneyActionDetailDto != null)
                {
                    moneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                        .FindBy(x => x.Id == moneyActionDetailDto.Id).FirstOrDefault();

                    moneyAction.Date = (DateTime)moneyActionDetailDto.Date;

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
                    _unitOfWork.GenericRepository<BoxAction>().Delete(oldBoxAction);

                }

                var boxAction = new BoxAction()
                {
                    Amount = dto.RecivingAmount.Value,
                    IsIncmoe = true,
                    CoinId = dto.CoinId,
                    Note = dto.Note,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);

                if (moneyAction != null)
                {
                    moneyAction.BoxAction = boxAction;
                    _unitOfWork.GenericRepository<MoneyAction>().Update(moneyAction);
                }
                else
                {
                    moneyAction = new MoneyAction()
                    {
                        Date = (DateTime)dto.MoenyActions.FirstOrDefault().Date,
                        Created = DateTime.Now,
                        CreatedBy = _appSession.GetUserName(),
                        TransactionId = dto.Id,
                        BoxAction = boxAction
                    };

                    _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                }


                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = dto.CoinId,
                    Amount = dto.RecivingAmount.Value,
                    MoenyAction = moneyAction,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);

                var treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    BranchCashFlow = branchCashFlow,
                    CoinId = dto.CoinId,
                    TreasuryId = treasuryId,
                    Amount = dto.RecivingAmount.Value,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = dto.CoinId,
                    ClientId = dto.SenderClientId.Value,
                    Amount = dto.RecivingAmount.Value,
                    MoenyAction = moneyAction,
                    Matched = false,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);


                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        private bool EditReciveFromCompanyForMainBoxMethond(OuterTransactionUpdateDto dto, decimal oldRecivingAmount, decimal oldRecivingAmount2, int oldCoinId, int oldTreasuryId, int oldSenderCompanyId, decimal oldSenderCompanyCommission)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();

                int? boxActionId;

                var moneyActionDetailDto = dto.MoenyActions.FirstOrDefault(x => x.Id != null);
                if (moneyActionDetailDto != null)
                {
                    boxActionId = moneyActionDetailDto.Id;

                    var oldMoneyAction = _unitOfWork.GenericRepository<MoneyAction>()
                   .FindBy(x => x.Id == moneyActionDetailDto.Id).FirstOrDefault();

                    if (oldMoneyAction != null)
                    {
                        var oldBoxAction = _unitOfWork.GenericRepository<BoxAction>()
                        .FindBy(x => x.Id == boxActionId).FirstOrDefault();
                        if (oldBoxAction != null)
                        {
                            _unitOfWork.GenericRepository<BoxAction>().Delete(oldBoxAction);
                        }

                        var oldBranchCashFlow = _unitOfWork.GenericRepository<BranchCashFlow>()
                            .FindBy(x => x.MonyActionId == oldMoneyAction.Id).FirstOrDefault();
                        if (oldBranchCashFlow != null)
                        {
                            _unitOfWork.GenericRepository<BranchCashFlow>().Delete(oldBranchCashFlow);
                        }

                        #region Branch Cash
                        var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(x => x.BranchId == branchId && x.CoinId == dto.CoinId).FirstOrDefault();
                        if (branchCash != null)
                        {
                            if (dto.CoinId == oldCoinId)
                            {
                                branchCash.Total -= oldRecivingAmount;
                            }
                            else
                            {
                                var oldBranchCash = _unitOfWork.GenericRepository<BranchCash>()
                                    .FindBy(x => x.BranchId == BranchHelper.Id && x.CoinId == oldCoinId).FirstOrDefault();

                                if (oldBranchCash != null)
                                {
                                    oldBranchCash.Total -= oldRecivingAmount;

                                    _unitOfWork.GenericRepository<BranchCash>().Update(oldBranchCash);
                                }
                            }

                            branchCash.Total += dto.RecivingAmount.Value;
                            branchCash.ModifiedBy = _appSession.GetUserName();
                            _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);

                        }

                        #endregion

                        var oldTreasuryMoneyAction = _unitOfWork.GenericRepository<TreasuryMoneyAction>()
                        .FindBy(x => x.BranchCashFlow.MonyActionId == oldMoneyAction.Id).FirstOrDefault();
                        if (oldTreasuryMoneyAction != null)
                        {
                            _unitOfWork.GenericRepository<TreasuryMoneyAction>().Delete(oldTreasuryMoneyAction);
                        }

                        #region Treasury Cash

                        var treasuryCash = _unitOfWork.GenericRepository<TreasuryCash>()
                            .FindBy(x => x.TreasuryId == treasuryId && x.CoinId == dto.CoinId).FirstOrDefault();

                        if (treasuryCash != null)
                        {
                            if (treasuryCash.CoinId == oldCoinId && treasuryId == oldTreasuryId)
                            {
                                treasuryCash.Total -= oldRecivingAmount;

                            }
                            else
                            {
                                var oldTreasuryCash = _unitOfWork.GenericRepository<TreasuryCash>()
                                .FindBy(x => x.TreasuryId == oldTreasuryId && x.CoinId == oldCoinId).FirstOrDefault();
                                if (oldTreasuryCash != null)
                                {
                                    oldTreasuryCash.Total -= oldRecivingAmount;
                                }
                            }

                            treasuryCash.Total += dto.RecivingAmount.Value;
                            treasuryCash.ModifiedBy = _appSession.GetUserName();
                            _unitOfWork.GenericRepository<TreasuryCash>().Update(treasuryCash);
                        }

                        #endregion

                        var oldCompanyCashFlow = _unitOfWork.GenericRepository<CompanyCashFlow>()
                            .FindBy(x => x.CompanyId == oldSenderCompanyId && x.CoinId == oldCoinId).FirstOrDefault();

                        if (oldCompanyCashFlow != null)
                        {
                            _unitOfWork.GenericRepository<CompanyCashFlow>().Delete(oldCompanyCashFlow);
                        }

                        #region Company Cash
                        var companyCash = _unitOfWork.GenericRepository<CompanyCash>()
                            .FindBy(x => x.CompanyId == dto.SenderCompanyId && x.CoinId == dto.CoinId).FirstOrDefault();
                        if (companyCash != null)
                        {
                            if (oldCoinId == dto.CoinId && dto.SenderCompanyId == oldSenderCompanyId)
                            {
                                companyCash.Total += oldRecivingAmount;
                                companyCash.Total -= oldSenderCompanyCommission;
                            }
                            else
                            {
                                var oldCompanyCash = _unitOfWork.GenericRepository<CompanyCash>()
                                    .FindBy(x => x.CompanyId == oldSenderCompanyId && x.CoinId == oldCoinId).FirstOrDefault();

                                if (oldCompanyCash != null)
                                {
                                    oldCompanyCash.Total += oldRecivingAmount;
                                    oldCompanyCash.Total -= oldSenderCompanyCommission;

                                    oldCompanyCash.ModifiedBy = _appSession.GetUserName();

                                    _unitOfWork.GenericRepository<CompanyCash>().Update(oldCompanyCash);
                                }
                            }

                            companyCash.Total -= dto.RecivingAmount.Value;
                            if (dto.SenderCompanyComission != null && dto.SenderCompanyComission != 0)
                                companyCash.Total += (decimal)dto.SenderCompanyComission;
                            companyCash.ModifiedBy = _appSession.GetUserName();
                        }
                        _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);

                        #endregion

                        _unitOfWork.GenericRepository<MoneyAction>().Delete(oldMoneyAction);
                    }

                    var boxAction = new BoxAction()
                    {
                        Amount = dto.RecivingAmount.Value,
                        IsIncmoe = true,
                        CoinId = dto.CoinId,
                        Note = dto.Note,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                    var moneyAction = new MoneyAction()
                    {
                        Date = DateTime.Now,
                        BoxActionsId = boxAction.Id,
                        TransactionId = dto.Id,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);

                    var branchCashFlow = new BranchCashFlow()
                    {
                        BranchId = branchId,
                        CoinId = dto.CoinId,
                        Amount = dto.RecivingAmount.Value,
                        MonyActionId = moneyAction.Id,
                        TreasuryId = treasuryId,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);

                    var treasuryMoneyAction = new TreasuryMoneyAction()
                    {
                        BranchCashFlowId = branchCashFlow.Id,
                        CoinId = dto.CoinId,
                        TreasuryId = treasuryId,
                        Amount = dto.RecivingAmount.Value,
                        CreatedBy = _appSession.GetUserName()
                    };

                    var companyCashFlow = new CompanyCashFlow()
                    {
                        CoinId = dto.CoinId,
                        CompanyId = dto.SenderClientId.Value,
                        Amount = dto.RecivingAmount.Value,
                        MoneyActionId = moneyAction.Id,
                        Matched = false,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);

                    _unitOfWork.Save();
                    _unitOfWork.Commit();

                }

                return true;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return false;
            }
        }

        private void DeleteIncomeBoxAction(MoneyAction moneyAction)
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
            _unitOfWork.GenericRepository<BoxAction>().Delete(oldBoxAction);

            _unitOfWork.GenericRepository<MoneyAction>().Delete(moneyAction);
        }
        #endregion

    }
}
