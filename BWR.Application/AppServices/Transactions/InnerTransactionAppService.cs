using AutoMapper;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Client;
using BWR.Application.Dtos.Company;
using BWR.Application.Dtos.Setting.Attachment;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Dtos.Setting.Country;
using BWR.Application.Dtos.Transaction.InnerTransaction;
using BWR.Application.Interfaces.Shared;
using BWR.Application.Interfaces.Transaction;
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

                innerTransactionsDto= Mapper.Map<List<Transaction>, List<InnerTransactionDto>>(innerTransactions);
            }
            catch (Exception ex)
            {
                Infrastructure.Exceptions.Tracing.SaveException(ex);
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
                Infrastructure.Exceptions.Tracing.SaveException(ex);
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
                    CreatedBy=_appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<IncomeTransactionCollection>().Insert(incomeTransactionCollection);

                foreach (var innerTransacrion in innerTransacrions.Transactions)
                {
                    var moneyAction = IncomeTrasactionForClient(innerTransacrion, mainCompanyId, incomeTransactionCollection);

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

        private MoneyAction IncomeTrasactionForClient(InnerTransactionInsertDto dto, int mainCompayId, IncomeTransactionCollection incomeTransactionCollection)
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

            var moneyAction = new MoneyAction();
            moneyAction.Transaction = transaction;
            moneyAction.CreatedBy = _appSession.GetUserName();
            _unitOfWork.GenericRepository<MoneyAction>().Insert(moneyAction);
            return moneyAction;
        }

        private Client GetClient(ClientForTransactionDto client)
        {
            if (client.Id != 0)
            {
                if(_unitOfWork.GenericRepository<Client>().GetAll().Any(x=>x.Id == client.Id))
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
                CreatedBy=_appSession.GetUserName()
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
                Amount = dto.Amount,
                Matched = false,
                MoenyAction = moneyAction
            };
            _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCahsFlow);
        }

        
    }
}
