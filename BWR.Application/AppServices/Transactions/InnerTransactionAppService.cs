using AutoMapper;
using BWR.Application.Common;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Client;
using BWR.Application.Dtos.Company;
using BWR.Application.Dtos.Setting.Attachment;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Dtos.Setting.Country;
using BWR.Application.Dtos.Statement;
using BWR.Application.Dtos.Transaction.InnerTransaction;
using BWR.Application.Interfaces.Shared;
using BWR.Application.Interfaces.Transaction;
using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using BWR.Domain.Model.Treasures;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Attachment = BWR.Domain.Model.Settings.Attachment;

namespace BWR.Application.AppServices.Transactions
{
    public class InnerTransactionAppService : IInnerTransactionAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;
        public InnerTransactionAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<InnerTransactionDto> GetTransactions()
        {
            IList<InnerTransactionDto> innerTransactionsDto = new List<InnerTransactionDto>();

            try
            {
                var innerTransactions = _unitOfWork.GenericRepository<Transaction>().FindBy(x => x.TransactionType == TransactionType.ImportTransaction).ToList();

                innerTransactionsDto = Mapper.Map<List<Transaction>, List<InnerTransactionDto>>(innerTransactions);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return innerTransactionsDto;
        }

        public InnerTransactionDto GetById(int id)
        {
            InnerTransactionDto innerTransactionDto = new InnerTransactionDto();

            try
            {
                var innerTransaction = _unitOfWork.GenericRepository<Transaction>()
                    .FindBy(x => x.Id == id).FirstOrDefault();

                innerTransactionDto = Mapper.Map<Transaction, InnerTransactionDto>(innerTransaction);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return innerTransactionDto;
        }

        public InnerTransactionInsertInitialDto InitialInputData()
        {
            var currentTreasuryId = _appSession.GetCurrentTreasuryId();
            var treasuryCashes = _unitOfWork.GenericRepository<TreasuryCash>().FindBy(x => x.TreasuryId == currentTreasuryId).ToList();
            var coins = treasuryCashes.Select(x => new CoinForDropdownDto() { Name = x.Coin.Name, Id = x.Coin.Id }).ToList();

            var countries = _unitOfWork.GenericRepository<Country>().FindBy(x => x.IsEnabled == true)
                .Select(x => new CountryForDropdownDto() { Id = x.Id, Name = x.Name }).ToList();

            var normalClients = _unitOfWork.GenericRepository<Client>().FindBy(x => x.ClientType == ClientType.Normal)
                .Select(x => new ClientDto() { Id = x.Id, FullName = x.FullName, IsEnabled = x.IsEnabled }).ToList();

            var clients = _unitOfWork.GenericRepository<Client>().FindBy(x => x.ClientType == ClientType.Client)
                .Select(x => new ClientDto() { Id = x.Id, FullName = x.FullName, IsEnabled = x.IsEnabled }).ToList();

            var companies = _unitOfWork.GenericRepository<Company>().GetAll()
                .Select(x => new CompanyForDropdownDto() { Id = x.Id, Name = x.Name }).ToList();

            var attachments = _unitOfWork.GenericRepository<Attachment>().GetAll()
                .Select(x => new AttachmentForDropdownDto() { Id = x.Id, Name = x.Name }).ToList();

            var outerTransactionInsertInputDto = new InnerTransactionInsertInitialDto()
            {
                Coins = coins,
                Countries = countries,
                NormalClients = normalClients,
                Clients = clients,
                TreasuryId = currentTreasuryId,
                Companies = companies,
                Attachments = attachments
            };

            return outerTransactionInsertInputDto;
        }

        public InnerTransactionUpdateDto GetForEdit(int transactionId)
        {
            InnerTransactionUpdateDto dto = new InnerTransactionUpdateDto();
            try
            {
                var transaction = _unitOfWork.GenericRepository<Transaction>().GetById(transactionId);
                if (transaction != null)
                {
                    dto.MainCompanyId = transaction.ReceiverCompanyId != null ? transaction.ReceiverCompanyId.Value : 0;
                    dto.Note = transaction.Note;
                    dto.OurComission = transaction.OurComission;
                    dto.TypeOfPay = transaction.TypeOfPay;
                    dto.Id = transaction.Id;
                    dto.AgentCommission = transaction.ReciverClientCommission != null ? transaction.ReciverClientCommission.Value : 0;
                    dto.AgentId = transaction.ReciverClientId != null ? transaction.ReciverClientId.Value : 0;
                    dto.CoinId = transaction.CoinId;
                    dto.Amount = transaction.Amount;
                    dto.Date = transaction.MoenyActions.FirstOrDefault().Date;
                    if (transaction.SenderClient != null)
                    {
                        dto.SenderClient = new ClientForTransactionDto()
                        {
                            Address = transaction.SenderClient.Address,
                            Id = transaction.SenderClient.Id,
                            Name = transaction.SenderClient.FullName,
                            Phone = transaction.SenderClient.ClientPhones.Any() ? transaction.SenderClient.ClientPhones.LastOrDefault().Phone : string.Empty
                        };

                        if (transaction.ReciverClient != null)
                        {
                            dto.ReceiverClient = new ClientForTransactionDto()
                            {
                                Address = transaction.ReciverClient.Address,
                                Name = transaction.ReciverClient.FullName,
                                Id = transaction.ReciverClient.Id,
                                Phone = transaction.ReciverClient.ClientPhones.Any() ? transaction.ReciverClient.ClientPhones.LastOrDefault().Phone : string.Empty
                            };
                        }
                        if (transaction.ReceiverCompany != null)
                        {
                            dto.SenderCompany = new CompanySenderDto()
                            {
                                CompanyCommission = transaction.SenderCompanyComission != null ? transaction.SenderCompanyComission.Value : 0,
                                CompanyId = transaction.SenderCompanyId != null ? transaction.SenderCompanyId.Value : 0,
                                ReciverClinet = new ClientForTransactionDto()
                                {
                                    Address = transaction.ReciverClient.Address,
                                    Id = transaction.ReciverClient.Id,
                                    Name = transaction.ReciverClient.FullName,
                                    Phone = transaction.ReciverClient.ClientPhones.Any() ? transaction.ReciverClient.ClientPhones.LastOrDefault().Phone : string.Empty
                                }

                            };
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return dto;
        }

        #region Insert

        public bool SaveInnerTransactions(InnerTransactionInsertListDto innerTransacrions)
        {
            try
            {
                _unitOfWork.CreateTransaction();

                var mainCompanyId = innerTransacrions.MainCompanyId;
                var note = innerTransacrions.Note;
                //new line
                var mainCompnayCashe = _unitOfWork.GenericRepository<CompanyCash>().FindBy(x => x.CompanyId == mainCompanyId);
                var incomeTransactionCollection = new IncomeTransactionCollection()
                {
                    CompnayId = mainCompanyId,
                    Note = note,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<IncomeTransactionCollection>().Insert(incomeTransactionCollection);

                foreach (var innerTransacrion in innerTransacrions.Transactions)
                {
                    var moneyAction = IncomeTrasactionForClient(innerTransacrion, mainCompanyId, innerTransacrions.Date, incomeTransactionCollection);

                    switch (innerTransacrion.TypeOfPay)
                    {
                        case TypeOfPay.Cash:
                            break;
                        case TypeOfPay.ClientsReceivables:
                            {
                                AgentBalnaceArbitrage(innerTransacrion, moneyAction);
                            }
                            break;
                        case TypeOfPay.CompaniesReceivables:
                            {
                                CompanyBlanceArbitrage(innerTransacrion, moneyAction);
                            }
                            break;
                        default:
                            {
                                return false;
                            }
                    }
                    MaiCompanyBalanceArbitrage(mainCompnayCashe.Where(c => c.CoinId == innerTransacrion.CoinId).First(), innerTransacrion, mainCompanyId, moneyAction);
                }
                var date = innerTransacrions.Date;
                var coinIdCollection = innerTransacrions.Transactions.Select(c => c.CoinId).Distinct().ToList();
                foreach (var coinId in coinIdCollection)
                {
                    var mainCompanyCashFlows = _unitOfWork.GenericRepository<CompanyCashFlow>().FindBy(c => c.CoinId == coinId && c.Matched == true && c.CompanyId == mainCompanyId && c.MoenyAction.Date > date).ToList();
                    foreach (var item in mainCompanyCashFlows)
                    {
                        item.Matched = false;
                        item.UserMatched = null;
                        item.CompanyUserMatched = null;
                        _unitOfWork.GenericRepository<CompanyCashFlow>().Update(item);
                    }
                }
                /// delete matching for clients
                {
                    var clientTransaction = incomeTransactionCollection.Transactions.Where(c => c.TypeOfPay == TypeOfPay.ClientsReceivables);
                    var transactionsGroupByCoin = clientTransaction.GroupBy(c => c.CoinId);
                    foreach (var groupByCoin in transactionsGroupByCoin)
                    {

                        var transactionsGroupByCoinAndAgent = groupByCoin.GroupBy(c => c.ReciverClientId);

                        foreach (var agentKey in transactionsGroupByCoinAndAgent)
                        {
                            var matchedCashFlows = _unitOfWork.GenericRepository<ClientCashFlow>().FindBy(c => c.Matched == true && c.CoinId == groupByCoin.Key && c.ClientId == agentKey.Key && c.MoenyAction.Date >= innerTransacrions.Date);
                            foreach (var item in matchedCashFlows)
                            {
                                item.Matched = false;
                                _unitOfWork.GenericRepository<ClientCashFlow>().Update(item);
                            }
                        }
                    }
                }
                {
                    var companyTrasaction = incomeTransactionCollection.Transactions.Where(c => c.TypeOfPay == TypeOfPay.CompaniesReceivables);
                    var transactionsGroupByCoin = companyTrasaction.GroupBy(c => c.CoinId);
                    foreach (var groupByCoin in transactionsGroupByCoin)
                    {
                        var trnsactionsGroupByCoinandCompany = groupByCoin.GroupBy(c => c.ReceiverCompanyId);
                        foreach (var companyGroup in trnsactionsGroupByCoinandCompany)
                        {
                            var mathcingCashFlow = _unitOfWork.GenericRepository<CompanyCashFlow>().FindBy(c => c.Matched == true && c.CoinId == groupByCoin.Key && c.CompanyId == companyGroup.Key && c.MoenyAction.Date >= innerTransacrions.Date);
                            foreach (var item in mathcingCashFlow)
                            {
                                item.Matched = false;
                                item.UserMatched = null;
                                item.CompanyUserMatched = null;
                                _unitOfWork.GenericRepository<CompanyCashFlow>().Update(item);
                            }
                        }
                    }
                }


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

        private MoneyAction IncomeTrasactionForClient(InnerTransactionInsertDto dto, int mainCompayId, DateTime date, IncomeTransactionCollection incomeTransactionCollection)
        {
            int branchId = BranchHelper.Id;
            int treasuryId = _appSession.GetCurrentTreasuryId();
            var transaction = new Transaction();
            transaction.Reason = "";
            transaction.SenderClient = GetClient(dto.Sender);
            transaction.SenderBranchId = branchId;
            transaction.ReceiverBranchId = branchId;
            transaction.TreaseryId = treasuryId;
            transaction.IncomeTransactionCollection = incomeTransactionCollection;
            switch (dto.TypeOfPay)
            {
                case TypeOfPay.Cash:
                    {
                        transaction.ReciverClient = GetClient(dto.ReciverClinet);
                        transaction.TypeOfPay = TypeOfPay.Cash;
                        transaction.Deliverd = false;
                    }
                    break;
                case TypeOfPay.ClientsReceivables:
                    {
                        transaction.ReciverClientId = dto.AgentId;
                        transaction.ReciverClientCommission = dto.AgentCommission;
                        transaction.TypeOfPay = TypeOfPay.ClientsReceivables;
                        transaction.Deliverd = true;
                    }
                    break;
                case TypeOfPay.CompaniesReceivables:
                    {
                        transaction.SenderCompanyId = dto.ReciverCompany.CompanyId;
                        transaction.SenderCompanyComission = dto.ReciverCompany.CompanyCommission;
                        transaction.ReciverClient = GetClient(dto.ReciverCompany.ReciverClinet);
                        transaction.TypeOfPay = TypeOfPay.CompaniesReceivables;
                        transaction.Deliverd = true;
                    }
                    break;
            }
            transaction.CoinId = dto.CoinId;
            transaction.ReceiverCompanyId = mainCompayId;
            transaction.Amount = dto.Amount;

            transaction.TransactionType = TransactionType.ImportTransaction;
            //transaction.Note = note;
            transaction.OurComission = dto.OurComission;
            transaction.TransactionsStatus = TransactionStatus.NotNotified;
            transaction.CreatedBy = _appSession.GetUserName();
            _unitOfWork.GenericRepository<Transaction>().Insert(transaction);

            var moneyAction = new MoneyAction
            {
                Transaction = transaction,
                Date = date,
                CreatedBy = _appSession.GetUserName()
            };
            _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
            return moneyAction;
        }

        private void CompanyBlanceArbitrage(InnerTransactionInsertDto dto, MoneyAction moneyAction)
        {
            var companyCah = _unitOfWork.GenericRepository<CompanyCash>()
                .FindBy(c => c.CoinId == dto.CoinId && c.CompanyId == dto.ReciverCompany.CompanyId)
                .FirstOrDefault();

            companyCah.Total += dto.Amount;
            companyCah.Total += dto.ReciverCompany.CompanyCommission;
            companyCah.ModifiedBy = _appSession.GetUserName();
            _unitOfWork.GenericRepository<CompanyCash>().Update(companyCah);

            var companyCahsFlow = new CompanyCashFlow()
            {
                MoenyAction = moneyAction,
                Total = companyCah.Total,
                Amount = dto.Amount + dto.ReciverCompany.CompanyCommission,
                Matched = false,
                CompanyId = dto.ReciverCompany.CompanyId,
                CoinId = dto.CoinId,
                CreatedBy = _appSession.GetUserName()
            };
            _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCahsFlow);
        }

        private void AgentBalnaceArbitrage(InnerTransactionInsertDto dto, MoneyAction moneyAction)
        {
            var clientCash = _unitOfWork.GenericRepository<ClientCash>()
                .FindBy(c => c.CoinId == dto.CoinId && c.ClientId == dto.AgentId)
                .FirstOrDefault();

            clientCash.Total += dto.Amount;
            clientCash.Total += dto.AgentCommission;
            clientCash.ModifiedBy = _appSession.GetUserName();
            _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);

            var clientCashFlow = new ClientCashFlow()
            {
                ClientId = dto.AgentId,
                CoinId = dto.CoinId,
                Total = clientCash.Total,
                Amount = dto.Amount,
                Matched = false,
                MoenyAction = moneyAction
            };
            _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);

        }

        private void MaiCompanyBalanceArbitrage(CompanyCash companyCash, InnerTransactionInsertDto dto, int mainCompanyId, MoneyAction moneyAction)
        {
            companyCash.Total -= (dto.Amount + dto.OurComission);
            companyCash.ModifiedBy = _appSession.GetUserName();
            _unitOfWork.GenericRepository<CompanyCash>().Update(companyCash);

            var companyCahsFlow = new CompanyCashFlow()
            {
                CoinId = dto.CoinId,
                CompanyId = mainCompanyId,
                Total = companyCash.Total,
                Amount = -dto.Amount,
                Matched = false,
                MoenyAction = moneyAction
            };
            _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCahsFlow);
        }

        #endregion

        #region Update

        public bool EditInnerTransaction(InnerTransactionUpdateDto dto)
        {
            try
            {

                _unitOfWork.CreateTransaction();

                var moneyAction = EditTransaction(dto);

                DeleteOldCashFlow(moneyAction);

                switch (dto.TypeOfPay)
                {
                    case TypeOfPay.Cash:
                        break;
                    case TypeOfPay.ClientsReceivables:
                        {
                            AddClientCashFlow(dto, moneyAction);
                        }
                        break;
                    case TypeOfPay.CompaniesReceivables:
                        {
                            AddCompanyCashFlow(dto, moneyAction);
                        }
                        break;
                    default:
                        {
                            return false;
                        }
                }

                EditMaiCompanyBalanceArbitrage(dto, moneyAction);

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

        private void DeleteOldCashFlow(MoneyAction moneyAction)
        {
            //Old Company Cash Flows
            var oldCompanyCashFlows = moneyAction.CompanyCashFlows.ToArray();
            for (int i = 0; i < oldCompanyCashFlows.Length; i++)
            {
                _unitOfWork.GenericRepository<CompanyCashFlow>().Delete(oldCompanyCashFlows[i]);
            }

            //Old Client Cash Flows
            var oldClientCashFlows = moneyAction.ClientCashFlows.ToArray();
            for (int i = 0; i < oldClientCashFlows.Length; i++)
            {
                _unitOfWork.GenericRepository<ClientCashFlow>().Delete(oldClientCashFlows[i]);
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

        private MoneyAction EditTransaction(InnerTransactionUpdateDto dto)
        {
            var transaction = _unitOfWork.GenericRepository<Transaction>().GetById(dto.Id);
            transaction.Reason = "";
            transaction.SenderClient = GetClient(dto.SenderClient);

            switch (dto.TypeOfPay)
            {
                case TypeOfPay.Cash:
                    {
                        transaction.ReciverClient = GetClient(dto.ReceiverClient);
                        transaction.TypeOfPay = TypeOfPay.Cash;
                        transaction.Deliverd = false;
                    }
                    break;
                case TypeOfPay.ClientsReceivables:
                    {
                        transaction.ReciverClientId = dto.AgentId;
                        transaction.ReciverClientCommission = dto.AgentCommission;
                        transaction.TypeOfPay = TypeOfPay.ClientsReceivables;
                        transaction.Deliverd = true;
                    }
                    break;
                case TypeOfPay.CompaniesReceivables:
                    {
                        transaction.SenderCompanyId = dto.SenderCompany.CompanyId;
                        transaction.SenderCompanyComission = dto.SenderCompany.CompanyCommission;
                        transaction.ReciverClient = GetClient(dto.SenderCompany.ReciverClinet);
                        transaction.TypeOfPay = TypeOfPay.CompaniesReceivables;
                        transaction.Deliverd = true;
                    }
                    break;
            }
            transaction.CoinId = dto.CoinId;
            transaction.ReceiverCompanyId = dto.MainCompanyId;
            transaction.Amount = dto.Amount;

            //transaction.TransactionType = TransactionType.ImportTransaction;
            transaction.Note = dto.Note;
            transaction.OurComission = dto.OurComission;
            //transaction.TransactionsStatus = TransactionStatus.NotNotified;
            transaction.ModifiedBy = _appSession.GetUserName();
            _unitOfWork.GenericRepository<Transaction>().Update(transaction);

            var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(x => x.TransactionId.Value == dto.Id)
                .FirstOrDefault();

            moneyAction.ModifiedBy = _appSession.GetUserName();
            moneyAction.Date = dto.Date;
            _unitOfWork.GenericRepository<MoneyAction>().Update(moneyAction);

            return moneyAction;
        }

        private void AddClientCashFlow(InnerTransactionUpdateDto dto, MoneyAction moneyAction)
        {
            var clientCashFlow = new ClientCashFlow()
            {
                ClientId = dto.AgentId,
                CoinId = dto.CoinId,
                Amount = dto.Amount,
                Matched = false,
                MoenyAction = moneyAction
            };
            _unitOfWork.GenericRepository<ClientCashFlow>().Insert(clientCashFlow);

        }

        private void AddCompanyCashFlow(InnerTransactionUpdateDto dto, MoneyAction moneyAction)
        {
            var companyCahsFlow = new CompanyCashFlow()
            {
                MoenyAction = moneyAction,
                Amount = dto.Amount + dto.SenderCompany.CompanyCommission,
                Matched = false,
                CompanyId = dto.SenderCompany.CompanyId,
                CoinId = dto.CoinId,
                CreatedBy = _appSession.GetUserName()
            };
            _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCahsFlow);
        }

        private void EditMaiCompanyBalanceArbitrage(InnerTransactionUpdateDto dto, MoneyAction moneyAction)
        {
            var oldCompanyCashFlow = _unitOfWork.GenericRepository<CompanyCashFlow>()
                .FindBy(x => x.CoinId == dto.CoinId && x.MoneyActionId == moneyAction.Id).FirstOrDefault();

            if (oldCompanyCashFlow != null)
            {
                _unitOfWork.GenericRepository<CompanyCashFlow>().Delete(oldCompanyCashFlow);
            }
            var companyCashFlow = new CompanyCashFlow()
            {
                CoinId = dto.CoinId,
                CompanyId = dto.MainCompanyId,
                Amount = -dto.Amount,
                Matched = false,
                MoenyAction = moneyAction
            };
            _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
        }

        #endregion

        private Client GetClient(ClientForTransactionDto client)
        {
            if (client.Id != 0)
            {
                if (_unitOfWork.GenericRepository<Client>().GetAll().Any(x => x.Id == client.Id))
                    return UpdateClient(client);
            }
            return AddNewClient(client);
        }

        private Client UpdateClient(ClientForTransactionDto clientDto)
        {
            var client = _unitOfWork.GenericRepository<Client>().GetById(clientDto.Id);
            client.Address = clientDto.Address;
            if (!client.ClientPhones.Any(c => c.Phone == clientDto.Phone))
            {
                if (!string.IsNullOrWhiteSpace(clientDto.Phone))
                {
                    var clientPhone = new ClientPhone()
                    {
                        Phone = clientDto.Phone,
                        ClientId = clientDto.Id,
                        CreatedBy = _appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<ClientPhone>().Insert(clientPhone);
                }
            }

            _unitOfWork.GenericRepository<Client>().Update(client);

            return client;
        }

        private Client AddNewClient(ClientForTransactionDto clientDto)
        {
            var client = new Client()
            {
                FullName = clientDto.Name,
                Address = clientDto.Address,
                CreatedBy = _appSession.GetUserName(),
                BranchId = BranchHelper.Id,
                ClientType = ClientType.Normal
            };

            _unitOfWork.GenericRepository<Client>().Insert(client);
            if (!string.IsNullOrWhiteSpace(clientDto.Phone))
            {
                var clientPhone = new ClientPhone()
                {
                    Phone = clientDto.Phone,
                    Client = client,
                    CreatedBy = _appSession.GetUserName()
                };

                _unitOfWork.GenericRepository<ClientPhone>().Insert(clientPhone);
            }

            return client;
        }

        public DataTablesDto InnerTransactionStatementDetailed(int draw, int start, int length, int? reciverCompanyId, TypeOfPay typeOfPay, int? reciverId, int? senderCompanyId, int? senderClientId, int? coinId, TransactionStatus transactionStatus, DateTime? from, DateTime? to, bool? isDelivered)
        {

            List<InnerTransactionStatementDetailedDto> innerTransactionDtos = new List<InnerTransactionStatementDetailedDto>();
            int total = 0;
            int filteredRec = 0;
            try
            {
                var innerTransaction = _unitOfWork.GenericRepository<Transaction>().FindBy(c => c.TransactionType == TransactionType.ImportTransaction, "SenderClient", "ReciverClient", "ReceiverCompany", "SenderCompany", "MoenyActions", "ReciverClient.ClientPhones");
                total = innerTransaction.Count();
                if (reciverCompanyId != null)
                {
                    innerTransaction = innerTransaction.Where(c => c.ReceiverCompanyId == reciverCompanyId);
                }
                if (typeOfPay != TypeOfPay.None)
                {
                    innerTransaction = innerTransaction.Where(c => c.TypeOfPay == typeOfPay);
                    if (typeOfPay == TypeOfPay.Cash || typeOfPay == TypeOfPay.ClientsReceivables)
                    {
                        if (reciverId != null)
                        {
                            innerTransaction = innerTransaction.Where(c => c.ReciverClientId == reciverId);
                        }
                    }
                    else if (typeOfPay == TypeOfPay.CompaniesReceivables)
                    {
                        if (reciverId != null)
                        {
                            innerTransaction = innerTransaction.Where(c => c.ReciverClientId == reciverId);
                        }
                        if (senderCompanyId != null)
                        {
                            innerTransaction = innerTransaction.Where(c => c.SenderCompanyId == senderCompanyId);
                        }
                    }
                }
                if (senderClientId != null)
                    innerTransaction = innerTransaction.Where(c => c.SenderClientId == senderClientId);
                if (coinId != null)
                {
                    innerTransaction = innerTransaction.Where(c => c.CoinId == coinId);
                }
                if (transactionStatus != TransactionStatus._0)
                    innerTransaction = innerTransaction.Where(c => c.TransactionsStatus == transactionStatus);
                if (from != null)
                {
                    innerTransaction = innerTransaction.Where(c => c.MoenyActions.ToList().First().Date >= from);
                }
                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    innerTransaction = innerTransaction.Where(c => c.MoenyActions.ToList().Last().Date <= dto);
                }
                filteredRec = innerTransaction.Count();
                innerTransaction = innerTransaction.Skip(start).Take(length);

                foreach (var item in innerTransaction.ToList())
                {
                    InnerTransactionStatementDetailedDto innerTransactionDto = new InnerTransactionStatementDetailedDto()
                    {
                        Amount = item.Amount,
                        CoinName = item.Coin.Name,
                        Date = item.MoenyActions.First().Date.ToString(),
                        IsDiliverd = (bool)item.IsDeleted,
                        TypeOfPay = item.TypeOfPay == TypeOfPay.Cash ? "نقدي" : item.TypeOfPay == TypeOfPay.ClientsReceivables ? "ذمم عملاء" : "ذمم شركات",
                        Id = item.Id,
                        MoneyActionId = item.MoenyActions.First().Id,
                        State = item.TransactionsStatus == TransactionStatus.Notified ? "مبلغ" : item.TransactionsStatus == TransactionStatus.NotNotified ? "غير مبلغ" : "بلا",
                        SenderName = item.SenderClient.FullName,
                        ReciverName = item.ReciverClient.FullName,
                        ReciverPhone = item.ReciverClient.ClientPhones.FirstOrDefault() != null ? item.ReciverClient.ClientPhones.FirstOrDefault().Phone : "",
                        SenderCompany = item.ReceiverCompany.Name,
                        ReciverAddress = item.ReciverClient.Address
                    };
                    innerTransactionDtos.Add(innerTransactionDto);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            DataTablesDto dt = new DataTablesDto(draw, innerTransactionDtos, filteredRec, total);
            return dt;

        }
        public DataTablesDto IncmoeTransactionGross(int draw, int start, int length, int? companyId, DateTime? from, DateTime? to)
        {
            int totalRecords = 0;
            int filteredRecords = 0;
            List<IncomeTransactionCollectionDto> incomeTransactionCollectionDtos = new List<IncomeTransactionCollectionDto>();
            try
            {
                var incomeTransactionCollection = _unitOfWork.GenericRepository<IncomeTransactionCollection>().GetAll("Transactions.MoenyActions", "Transactions.Coin", "Company");
                totalRecords = incomeTransactionCollection.Count();
                if (from != null)
                    incomeTransactionCollection = incomeTransactionCollection.Where(ic => ic.Transactions.Any(t => t.MoenyActions.Any(m => m.Date >= from)));
                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    incomeTransactionCollection = incomeTransactionCollection.Where(ic => ic.Transactions.Any(t => t.MoenyActions.Any(m => m.Date <= dto)));
                }
                if (companyId != null)
                {
                    incomeTransactionCollection = incomeTransactionCollection.Where(c => c.CompnayId == companyId);
                }
                filteredRecords = incomeTransactionCollection.Count();
                incomeTransactionCollection = incomeTransactionCollection.Skip(start).Take(length);
                foreach (var item in incomeTransactionCollection)
                {
                    var transactions = item.Transactions.GroupBy(c => c.Coin);
                    string amount = "";
                    foreach (var coinsandAmount in transactions)
                    {
                        amount += coinsandAmount.Key.Name + " :" + coinsandAmount.Sum(c => c.Amount);
                        amount += "<br>";
                    }

                    IncomeTransactionCollectionDto incomeTransactionCollectionDto = new IncomeTransactionCollectionDto()
                    {
                        Id = item.Id,
                        Company = item.Company.Name,
                        Date = item.GetDate.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")),
                        Amount = amount,
                        TransactionCount = item.Transactions.Count()
                    };
                    incomeTransactionCollectionDtos.Add(incomeTransactionCollectionDto);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return new DataTablesDto(draw, incomeTransactionCollectionDtos, filteredRecords, totalRecords);
        }

        public DataTablesDto TransactionDontDileverd(int draw, int start, int length, TransactionStatus transactionStatus, int? clientId, int? companyId, int? coinId, DateTime? from, DateTime? to)
        {
            List<TransactionDontDelivered> transactionDontDelivereds = new List<TransactionDontDelivered>();
            int total = 0;
            int filtedCount = 0;
            try
            {
                var incomeTransaction = _unitOfWork.GenericRepository<Transaction>().FindBy(c => c.Deliverd == false && c.TransactionType == TransactionType.ImportTransaction, "MoenyActions", "Coin", "ReceiverCompany", "SenderClient", "ReciverClient");
                total = incomeTransaction.Count();
                if (transactionStatus != TransactionStatus._0)
                    incomeTransaction = incomeTransaction.Where(c => c.TransactionsStatus == transactionStatus);

                if (companyId != null)
                    incomeTransaction = incomeTransaction.Where(c => c.ReceiverCompanyId == companyId);
                if (from != null)
                    incomeTransaction = incomeTransaction.Where(c => c.MoenyActions.ToList().First().Date >= from);

                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    incomeTransaction = incomeTransaction.Where(c => c.MoenyActions.ToList().First().Date <= dto);
                }
                if (coinId != null)
                    incomeTransaction = incomeTransaction.Where(c => c.CoinId == coinId);
                if (clientId != null)
                    incomeTransaction = incomeTransaction.Where(c => c.ReciverClientId == clientId);
                filtedCount = incomeTransaction.Count();
                incomeTransaction = incomeTransaction.Skip(start).Take(length);
                var incomeTransactionlist = incomeTransaction.ToList();
                foreach (var item in incomeTransactionlist)
                {
                    TransactionDontDelivered transactionDontDelivered = new TransactionDontDelivered()
                    {
                        Id = item.Id,
                        MoneyActionId = item.MoenyActions.First().Id,
                        Date = item.MoenyActions.First().Date.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")),
                        Amount = item.Amount,
                        Coin = item.Coin.Name,

                        Company = item.ReceiverCompany.Name,
                        TransactionStatus = item.TransactionsStatus == TransactionStatus.Notified ? "مبلغ" : item.TransactionsStatus == TransactionStatus.NotNotified ? "غير مبلغ" : "بلا",
                        SenderName = item.SenderClient.FullName,
                        ReciverName = item.ReciverClient.FullName,

                        Address = item.ReciverClient.Address,
                        Note = item.Note
                    };
                    transactionDontDelivereds.Add(transactionDontDelivered);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return new DataTablesDto(draw, transactionDontDelivereds, filtedCount, total);
        }
    }
}
