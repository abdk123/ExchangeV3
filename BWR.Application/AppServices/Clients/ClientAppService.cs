using AutoMapper;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Client;
using BWR.Application.Dtos.Common;
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
using System.Linq.Expressions;

namespace BWR.Application.AppServices.Clients
{
    public class ClientAppService : IClientAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IClientCashAppService _clientCashAppService;
        private readonly IClientPhoneAppService _clientPhoneAppService;
        private readonly IAppSession _appSession;
        
        public ClientAppService(
            IUnitOfWork<MainContext> unitOfWork,
            IClientCashAppService clientCashAppService,
            IClientPhoneAppService clientPhoneAppService,
            IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _clientCashAppService = clientCashAppService;
            _clientPhoneAppService = clientPhoneAppService;
            _appSession = appSession;
        }

        public IList<ClientDto> GetAll()
        {
            var clientsDtos = new List<ClientDto>();
            try
            {
                var clients = _unitOfWork.GenericRepository<Client>().FindBy(x => x.ClientType == ClientType.Client).ToList();
                clientsDtos = Mapper.Map<List<Client>, List<ClientDto>>(clients);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientsDtos;
        }

        public ClientDto GetById(int id)
        {
            ClientDto clientDto = null;
            try
            {
                var client = _unitOfWork.GenericRepository<Client>().GetById(id);
                if (client != null)
                {
                    clientDto = Mapper.Map<Client, ClientDto>(client);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientDto;
        }

        public IList<ClientDto> Get(Expression<Func<Client, bool>> predicate)
        {
            var clientsDtos = new List<ClientDto>();
            try
            {
                var clients = _unitOfWork.GenericRepository<Client>().FindBy(predicate).ToList();
                clientsDtos = Mapper.Map<List<Client>, List<ClientDto>>(clients);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientsDtos;
        }


        public ClientUpdateDto GetForEdit(int id)
        {
            ClientUpdateDto clientDto = null;
            try
            {
                var client = _unitOfWork.GenericRepository<Client>().GetById(id);
                if (client != null)
                {
                    clientDto = Mapper.Map<Client, ClientUpdateDto>(client);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientDto;
        }

        public ClientDto Insert(ClientInsertDto dto)
        {
            ClientDto clientDto = null;
            try
            {
                _unitOfWork.CreateTransaction();

                var client = Mapper.Map<ClientInsertDto, Client>(dto);
                client.CreatedBy = _appSession.GetUserName();
                client.IsEnabled = true;
                //client.ClientType = ClientType.Client;
                client.BranchId = BranchHelper.Id;

                _unitOfWork.GenericRepository<Client>().Insert(client);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                clientDto = Mapper.Map<Client, ClientDto>(client);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return clientDto;
        }

        public ClientDto Update(ClientUpdateDto dto)
        {
            ClientDto clientDto = null;
            try
            {
                var client = _unitOfWork.GenericRepository<Client>().GetById(dto.Id);
                Mapper.Map<ClientUpdateDto, Client>(dto, client);
                client.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Client>().Update(client);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                foreach (var clientCashDto in dto.ClientCashes)
                {
                    if (clientCashDto.Id == null || clientCashDto.Id == 0)
                    {
                        _clientCashAppService.Insert(clientCashDto);
                    }
                    else
                    {
                        _clientCashAppService.Update(clientCashDto);
                    }
                }

                CheckForDelete(dto.ClientPhones, client.ClientPhones);
                CheckForAdd(dto.ClientPhones);
                clientDto = Mapper.Map<Client, ClientDto>(client);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return clientDto;
        }

        public void Delete(int id)
        {
            try
            {
                var country = _unitOfWork.GenericRepository<Client>().GetById(id);
                if (country != null)
                {
                    //    var clientCashes = _unitOfWork.GenericRepository<ClientCash>().FindBy(x => x.ClientId == id).ToList();
                    //    var clientPhones = _unitOfWork.GenericRepository<ClientPhone>().FindBy(x => x.ClientId == id).ToList();



                    //    if (clientCashes.Any())
                    //    {
                    //        foreach (var clientCashe in clientCashes)
                    //        {
                    //            _clientCashAppService.Delete(clientCashe.Id);
                    //        }
                    //    }

                    //    if (clientCashes.Any())
                    //    {
                    //        foreach (var clientPhone in clientPhones)
                    //        {
                    //            _clientCashAppService.Delete(clientPhone.Id);
                    //        }
                    //    }

                    _unitOfWork.CreateTransaction();
                    _unitOfWork.GenericRepository<Client>().Delete(country);
                    _unitOfWork.Save();
                    _unitOfWork.Commit();
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        private void CheckForDelete(IList<ClientPhoneDto> clientPhoneDtos, IList<ClientPhone> clientPhones)
        {
            try
            {
                var deletedPhones = clientPhones.Where(x => !clientPhoneDtos.Select(y => y.Id).Contains(x.Id));
                if (deletedPhones.Any())
                {
                    foreach (var deletedPhone in deletedPhones)
                    {
                        _clientPhoneAppService.Delete(deletedPhone.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

        }

        private void CheckForAdd(IList<ClientPhoneDto> clientPhoneDtos)
        {
            try
            {
                var newClientPhoneDtos = clientPhoneDtos.Where(x => x.Id == 0 || x.Id == null);
                if (newClientPhoneDtos.Any())
                {
                    foreach (var newCountry in newClientPhoneDtos)
                    {
                        _clientPhoneAppService.Insert(newCountry);
                    }
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

        }

        public IList<Select2Dto<int>> GetSelect2(Expression<Func<Client, bool>> predicate = null)
        {

            var clientsDtos = new List<Select2Dto<int>>();
            try
            {

                if (predicate != null)
                {
                    var clients = _unitOfWork.GenericRepository<Client>().FindBy(predicate).ToList();
                    clientsDtos = Mapper.Map<List<Client>, List<Select2Dto<int>>>(clients);
                }
                else
                {
                    var clients = _unitOfWork.GenericRepository<Client>().GetAll().ToList();
                    clientsDtos = Mapper.Map<List<Client>, List<Select2Dto<int>>>(clients);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientsDtos;
        }
    }
}
