using AutoMapper;
using BWR.Application.Dtos.Client.ClientCashFlow;
using BWR.Application.Extensions;
using BWR.Application.Interfaces.Common;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Enums;
using BWR.Domain.Model.Settings;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BWR.Application.Interfaces.Client;
using System;
using BWR.Application.Dtos.Statement;

namespace BWR.Application.AppServices.Companies
{
    public class ClientCashFlowAppService : IClientCashFlowAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IMoneyActionAppService _moneyActionAppService;
        private readonly IAppSession _appSession;
        public ClientCashFlowAppService(
            IUnitOfWork<MainContext> unitOfWork,
            IMoneyActionAppService moneyActionAppService,
            IAppSession appSession,
            IClientAppService clientAppService,
            IClientCashAppService clientCashAppService)
        {
            _unitOfWork = unitOfWork;
            _moneyActionAppService = moneyActionAppService;
            _appSession = appSession;
        }

        public IList<ClientCashFlowOutputDto> Get(ClientCashFlowInputDto input)
        {
            var clientCashFlowsDtos = new List<ClientCashFlowOutputDto>();
            try
            {
                if (input.CoinId != 0)
                {
                    decimal? lastBalance = 0;
                    var clientCash = _unitOfWork.GenericRepository<ClientCash>().FindBy(x => x.CoinId == input.CoinId && x.ClientId == input.ClientId).FirstOrDefault();
                    if (clientCash != null)
                    {
                        lastBalance = clientCash.InitialBalance;
                    }
                    var clientCashFlows = _unitOfWork.GenericRepository<ClientCashFlow>()
                        .FindBy(x => x.CoinId.Equals(input.CoinId) && x.ClientId.Equals(input.ClientId), c => c.MoenyAction, c => c.MoenyAction.Clearing, c => c.MoenyAction.Clearing.ToClient
                        , c => c.MoenyAction.Clearing.FromClient
                        , c => c.MoenyAction.Clearing.FromCompany
                        , c => c.MoenyAction.Clearing.ToCompany
                        , c => c.MoenyAction.PublicMoney.PublicExpense
                        , c => c.MoenyAction.PublicMoney.PublicIncome).OrderBy(x => x.MoenyAction.Date);
                    var clientCashFlowsBeforeFromDate = clientCashFlows.Where(x => x.MoenyAction.Date < input.From);
                    if (clientCashFlowsBeforeFromDate.Any())
                    {
                        var lastClientCashFlowBeforeFromDate = clientCashFlowsBeforeFromDate.LastOrDefault();
                        lastBalance = lastClientCashFlowBeforeFromDate.Total;
                    }

                    clientCashFlowsDtos.Add(
                            new ClientCashFlowOutputDto()
                            {
                                Balance = lastBalance,
                                Type = "رصيد سابق"
                            });


                    var dataCashFlows = new List<ClientCashFlow>();

                    if (input.From != null && input.To != null)
                    {
                        dataCashFlows = clientCashFlows.Where(x => x.MoenyAction.Date >= input.From && x.MoenyAction.Date <= input.To).ToList();
                    }
                    else if (input.From == null && input.To != null)
                    {
                        dataCashFlows = clientCashFlows.Where(x => x.MoenyAction.Date <= input.To).ToList();
                    }   
                    else if (input.From != null && input.To == null)
                    {
                        dataCashFlows = clientCashFlows.Where(x => x.MoenyAction.Date >= input.From).ToList();
                    }
                    else
                    {
                        dataCashFlows = clientCashFlows.ToList();
                    }

                    foreach (var clientCashFlow in dataCashFlows)
                    {
                        var temp = new ClientCashFlowOutputDto()
                        {
                            Id = clientCashFlow.Id,
                            Balance = clientCashFlowsDtos.Last().Balance + clientCashFlow.Amount,
                            Amount = clientCashFlow.Amount,
                            SecondCommission = clientCashFlow.MoenyAction.ClientComission(input.ClientId),
                            Commission = clientCashFlow.MoenyAction.OurCommission(),
                            Type = clientCashFlow.MoenyAction.GetTypeName(Requester.Agent, clientCashFlow.ClientId),
                            Number = clientCashFlow.MoenyAction.GetActionId(),
                            Date = clientCashFlow.Created != null ? clientCashFlow.Created.Value.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")) : string.Empty,
                            Note = clientCashFlow.MoenyAction.GetNote(Requester.Agent, clientCashFlow.ClientId),
                            MoneyActionId = clientCashFlow.MoenyActionId,
                            Matched = clientCashFlow.Matched
                        };
                        temp.Balance += temp.SecondCommission;
                        temp.Balance -= temp.Commission;
                        
                        clientCashFlowsDtos.Add(temp);
                    }
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientCashFlowsDtos;
        }

        public ClientCashFlowDto GetById(int id)
        {
            ClientCashFlowDto clientCashFlowDto = null;
            try
            {
                var clientCashFlow = _unitOfWork.GenericRepository<ClientCashFlow>().GetById(id);
                if (clientCashFlow != null)
                {
                    clientCashFlowDto = Mapper.Map<ClientCashFlow, ClientCashFlowDto>(clientCashFlow);

                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientCashFlowDto;
        }

        public void Delete(int id)
        {
            try
            {
                var clientCashFlow = _unitOfWork.GenericRepository<ClientCashFlow>().GetById(id);
                if (clientCashFlow != null)
                {
                    _unitOfWork.GenericRepository<ClientCashFlow>().Delete(clientCashFlow);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        public IList<BalanceStatementDto> GetForStatement(int coinId, DateTime date)
        {
             date= date.AddHours(24);
            var clientCashFlowsDtos = new List<BalanceStatementDto>();
            try
            {
                var clients = _unitOfWork.GenericRepository<Client>().GetAll();
                foreach (var item in  clients)
                {
                    var initialBlance = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.CoinId == coinId&&c.ClientId==item.Id).First().InitialBalance;
                    var clientcashFlow = _unitOfWork.GenericRepository<ClientCashFlow>().FindBy(c => c.ClientId == item.Id && c.MoenyAction.Date <= date).ToList();
                    decimal total=0;    
                    if (clientcashFlow.Count != 0)
                    {
                        total = clientcashFlow.Sum(c => c.Amount);
                    }
                    total += initialBlance;
                    clientCashFlowsDtos.Add(new BalanceStatementDto()
                    {
                        Name = item.FullName,
                        Total = total,
                        Type = "عميل"
                    });
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientCashFlowsDtos;
        }

        public ClientMatchDto ConvertMatchingStatus(ClientMatchDto dto)
        {
            ClientMatchDto output = null;
            try
            {
                _unitOfWork.CreateTransaction();

                var clientCashFlow = _unitOfWork.GenericRepository<ClientCashFlow>().FindBy(x => x.Id == dto.ClientCashFlowId).FirstOrDefault();
                if (clientCashFlow != null)
                {
                    clientCashFlow.Matched = dto.Matched;
                }

                _unitOfWork.Save();
                _unitOfWork.Commit();

                output = dto;
            }
            catch(Exception ex)
            {
                _unitOfWork.Rollback();
            }

            return output;
        }
    }
}
