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

namespace BWR.Application.AppServices.Clients
{
    public class ClientPhoneAppService : IClientPhoneAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public ClientPhoneAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<ClientPhoneDto> GetAll()
        {
            var clientPhonesDtos = new List<ClientPhoneDto>();
            try
            {
                var clientPhones = _unitOfWork.GenericRepository<ClientPhone>().GetAll().ToList();
                clientPhonesDtos = Mapper.Map<List<ClientPhone>, List<ClientPhoneDto>>(clientPhones);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientPhonesDtos;
        }

        public ClientPhoneDto GetById(int id)
        {
            ClientPhoneDto clientPhoneDto = null;
            try
            {
                var clientPhone = _unitOfWork.GenericRepository<ClientPhone>().GetById(id);
                if (clientPhone != null)
                {
                    clientPhoneDto = Mapper.Map<ClientPhone, ClientPhoneDto>(clientPhone);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientPhoneDto;
        }

        public ClientPhoneDto Insert(ClientPhoneDto dto)
        {
            ClientPhoneDto clientPhoneDto = null;
            try
            {
                var clientPhone = Mapper.Map<ClientPhoneDto, ClientPhone>(dto);
                clientPhone.CreatedBy = _appSession.GetUserName();
                clientPhone.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<ClientPhone>().Insert(clientPhone);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                clientPhoneDto = Mapper.Map<ClientPhone, ClientPhoneDto>(clientPhone);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return clientPhoneDto;
        }

        public ClientPhoneDto Update(ClientPhoneDto dto)
        {
            ClientPhoneDto clientPhoneDto = null;
            try
            {
                var clientPhone = _unitOfWork.GenericRepository<ClientPhone>().GetById(dto.Id);
                Mapper.Map<ClientPhoneDto, ClientPhone>(dto, clientPhone);
                clientPhone.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<ClientPhone>().Update(clientPhone);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                clientPhoneDto = Mapper.Map<ClientPhone, ClientPhoneDto>(clientPhone);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return clientPhoneDto;
        }

        public void Delete(int id)
        {
            try
            {
                var clientPhone = _unitOfWork.GenericRepository<ClientPhone>().GetById(id);
                if (clientPhone != null)
                {
                    _unitOfWork.GenericRepository<ClientPhone>().Delete(clientPhone);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }


    }
}
