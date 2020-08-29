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

                _unitOfWork.GenericRepository<Transaction>().Insert(outerTransaction);

                _unitOfWork.Save();

                #region Money Action
                var moneyAction = new MoneyAction()
                {
                    TransactionId = outerTransaction.Id,
                    //Transaction = outerTransaction,
                    CreatedBy = _appSession.GetUserName()
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
                    //TransactionId = outerTransaction.Id,
                    Transaction = outerTransaction,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                #endregion

                #region Branch Cash
                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(x => x.BranchId == BranchHelper.Id && x.CoinId == dto.CoinId).FirstOrDefault();
                if (branchCash != null)
                {
                    branchCash.Total += dto.Amount + dto.OurComission;
                    branchCash.ModifiedBy = _appSession.GetUserName();
                    _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);
                }

                #endregion

                #region Branch Cash Flow
                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = BranchHelper.Id,
                    CreatedBy = _appSession.GetUserName(),
                    MoenyAction = moneyAction,
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
                    Amount =- dto.Amount
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
                    bool response = ReciveFromClientForMainBoxMethond(outerTransaction.Id, dto.SenderClientId.Value, dto.CoinId, (decimal)dto.RecivingAmount, dto.Note);
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
                    CreatedBy = _appSession.GetUserName()
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

                if (dto.RecivingAmount != null && dto.RecivingAmount != 0)
                {
                    bool response = ReciveFromClientForMainBoxMethond(outerTransaction.Id, dto.SenderClientId.Value, dto.CoinId, (decimal)dto.RecivingAmount, dto.Note);
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

        public bool ReciveFromClientForMainBoxMethond(int transactionId, int clientId, int coinId, decimal amount, string note = "")
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var branchId = BranchHelper.Id;
                var treasuryId = _appSession.GetCurrentTreasuryId();
                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.BranchId == branchId && c.CoinId == coinId).FirstOrDefault();
                var boxAction = new BoxAction()
                {
                    Amount = amount,
                    IsIncmoe = true,
                    CoinId = coinId,
                    Note = note,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<BoxAction>().Insert(boxAction);
                var moneyAction = new MoneyAction()
                {
                    BoxActionsId = boxAction.Id,
                    TransactionId = transactionId,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
                branchCash.Total += amount;
                branchCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);

                var branchCashFlow = new BranchCashFlow()
                {
                    BranchId = branchId,
                    CoinId = coinId,
                    Total = branchCash.Total,
                    Amount = amount,
                    MonyActionId = moneyAction.Id,
                    TreasuryId = treasuryId,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<BranchCashFlow>().Insert(branchCashFlow);

                var treuseryCash = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(c => c.CoinId == coinId && c.TreasuryId == treasuryId).FirstOrDefault();
                treuseryCash.Total += amount;
                treuseryCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<TreasuryCash>().Update(treuseryCash);
                var treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    BranchCashFlowId = branchCashFlow.Id,
                    CoinId = coinId,
                    Total = treuseryCash.Total,
                    TreasuryId = treasuryId,
                    Amount = amount,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);
                var clientCash = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.ClientId == clientId && c.CoinId == coinId).First();
                clientCash.Total += amount;
                clientCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);


                var clientCashFlow = new ClientCashFlow()
                {
                    CoinId = coinId,
                    ClientId = clientId,
                    Total = clientCash.Total,
                    Amount = amount,
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

    }
}
