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
            IAppSession appSession)
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
                        .FindBy(x => x.CoinId.Equals(input.CoinId) && x.ClientId.Equals(input.ClientId));

                    var clientCashFlowsBeforeFromDate = clientCashFlows.Where(x => x.Created.Value.Date < input.From);
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
                        dataCashFlows = clientCashFlows.Where(x => x.Created.Value.Date >= input.From && x.Created.Value.Date <= input.To).ToList();
                    }
                    else if (input.From == null && input.To != null)
                    {
                        dataCashFlows = clientCashFlows.Where(x => x.Created.Value.Date <= input.To).ToList();
                    }
                    else if (input.From != null && input.To == null)
                    {
                        dataCashFlows = clientCashFlows.Where(x => x.Created.Value.Date >= input.From).ToList();
                    }
                    else
                    {
                        dataCashFlows = clientCashFlows.ToList();
                    }

                    foreach (var clientCashFlow in dataCashFlows)
                    {
                        clientCashFlowsDtos.Add(
                            new ClientCashFlowOutputDto()
                            {
                                Id = clientCashFlow.Id,
                                Balance = clientCashFlow.Total,
                                Amount = clientCashFlow.Amount,
                                SecondCommission = clientCashFlow.MoenyAction.ClientComission(input.ClientId),
                                Commission = clientCashFlow.MoenyAction.OurCommission(),
                                Type = clientCashFlow.MoenyAction.GetTypeName(Requester.Agent, clientCashFlow.ClientId),
                                Number = clientCashFlow.MoenyAction.GetActionId(),
                                Date = clientCashFlow.Created != null ? clientCashFlow.Created.Value.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")) : string.Empty,
                                Note = clientCashFlow.MoenyAction.GetNote(Requester.Agent, clientCashFlow.ClientId)
                            });
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

        public IList<BalanceStatementDto> GetForStatement(int coinId, DateTime? date)
        {
            var clientCashFlowsDtos = new List<BalanceStatementDto>();
            try
            {
                var clientCashFlows = _unitOfWork.GenericRepository<ClientCashFlow>()
                        .FindBy(x => x.CoinId.Equals(coinId));

                if (date != null)
                {
                    clientCashFlows = clientCashFlows.Where(x => x.Created <= date);
                }

                clientCashFlowsDtos = (from c in clientCashFlows
                                       select new BalanceStatementDto()
                                       {
                                           Name = c.Client != null ? c.Client.FullName : string.Empty,
                                           Total = c.Total,
                                           Type = "العميل "
                                       }).ToList();

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientCashFlowsDtos;
        }
    }
}
