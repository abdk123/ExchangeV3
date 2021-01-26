using AutoMapper;
using BWR.Application.Dtos.Client;
using BWR.Application.Interfaces.Client;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Clients;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BWR.Application.AppServices.Clients
{
    public class ClientCashAppService : IClientCashAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public ClientCashAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<ClientCashDto> GetAll()
        {
            var clientCashsDtos = new List<ClientCashDto>();
            try
            {
                var clientCashs = _unitOfWork.GenericRepository<ClientCash>().GetAll().ToList();
                clientCashsDtos = Mapper.Map<List<ClientCash>, List<ClientCashDto>>(clientCashs);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientCashsDtos;
        }

        public ClientCashDto GetById(int id)
        {
            ClientCashDto clientCashDto = null;
            try
            {
                var clientCash = _unitOfWork.GenericRepository<ClientCash>().GetById(id);
                if (clientCash != null)
                {
                    clientCashDto = Mapper.Map<ClientCash, ClientCashDto>(clientCash);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientCashDto;
        }

        public IList<ClientCashesDto> GetClientCashes(int clientId)
        {
            IList<ClientCashesDto> clientBalanceDtos = new List<ClientCashesDto>();

            try
            {
                var clientCashes = _unitOfWork.GenericRepository<ClientCash>().FindBy(x => x.ClientId == clientId).ToList();

                foreach (var clientCash in clientCashes)
                {

                    var total = _unitOfWork.GenericRepository<ClientCashFlow>().FindBy(c => c.ClientId == clientId && c.CoinId == clientCash.CoinId).Sum(c => c.RealAmount);
                    total += clientCash.InitialBalance;
                    var clientBalanceDto = new ClientCashesDto()
                    {
                        Id = clientCash.Id,
                        CoinId = clientCash.CoinId,
                        CoinName = clientCash.Coin != null ? clientCash.Coin.Name : string.Empty,
                        ClientId = clientCash.ClientId,
                        InitialBalance = clientCash.InitialBalance,
                        Total = total,
                        MaxCreditor = clientCash.MaxCreditor,
                        MaxDebit = clientCash.MaxDebit
                    };

                    decimal onHim = 0;
                    decimal forHim = 0;
                    if (clientBalanceDto.Total > 0)
                    {
                        forHim = (clientBalanceDto.Total * 100) / 100;
                    }
                    else if (clientBalanceDto.Total < 0)
                    {
                        onHim = (clientBalanceDto.Total * 100) / 100;
                    }

                    clientBalanceDto.ForHim = forHim;
                    clientBalanceDto.OnHim = onHim;

                    clientBalanceDtos.Add(clientBalanceDto);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return clientBalanceDtos;
        }

        public ClientCashDto Insert(ClientCashDto dto)
        {
            ClientCashDto clientCashDto = null;
            try
            {
                var clientCash = Mapper.Map<ClientCashDto, ClientCash>(dto);
                clientCash.CreatedBy = _appSession.GetUserName();
                clientCash.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<ClientCash>().Insert(clientCash);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                clientCashDto = Mapper.Map<ClientCash, ClientCashDto>(clientCash);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return clientCashDto;
        }

        public ClientCashDto Update(ClientCashDto dto)
        {
            ClientCashDto clientCashDto = null;
            try
            {
                var clientCash = _unitOfWork.GenericRepository<ClientCash>().GetById(dto.Id);
                Mapper.Map<ClientCashDto, ClientCash>(dto, clientCash);
                clientCash.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                clientCashDto = Mapper.Map<ClientCash, ClientCashDto>(clientCash);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return clientCashDto;
        }

        public ClientCashDto UpdateBalance(ClientCashesDto dto)
        {
            ClientCashDto clientCashDto = null;
            try
            {
                var clientCash = _unitOfWork.GenericRepository<ClientCash>().GetById(dto.Id);
                Mapper.Map<ClientCashesDto, ClientCash>(dto, clientCash);
                clientCash.ModifiedBy = _appSession.GetUserName();
                
                _unitOfWork.GenericRepository<ClientCash>().Update(clientCash);
                _unitOfWork.Save();

                clientCashDto = Mapper.Map<ClientCash, ClientCashDto>(clientCash);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return clientCashDto;
        }

        public void Delete(int id)
        {
            try
            {
                var clientCash = _unitOfWork.GenericRepository<ClientCash>().GetById(id);
                if (clientCash != null)
                {
                    _unitOfWork.GenericRepository<ClientCash>().Delete(clientCash);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        public Task<ClientCashDto> InsertAsync(ClientCashDto dto)
        {
            return Task.Factory.StartNew(()=> {
                ClientCashDto clientCashDto = null;
                try
                {
                    var clientCash = Mapper.Map<ClientCashDto, ClientCash>(dto);
                    clientCash.CreatedBy = _appSession.GetUserName();
                    clientCash.IsEnabled = true;
                    _unitOfWork.CreateTransaction();

                    _unitOfWork.GenericRepository<ClientCash>().Insert(clientCash);
                    _unitOfWork.Save();

                    _unitOfWork.Commit();

                    clientCashDto = Mapper.Map<ClientCash, ClientCashDto>(clientCash);
                }
                catch (Exception ex)
                {
                    Tracing.SaveException(ex);
                    _unitOfWork.Rollback();
                }
                return clientCashDto;
            });
        }
    }
}
