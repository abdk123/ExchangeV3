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
    public class ClientAttatchmentAppService : IClientAttatchmentAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public ClientAttatchmentAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<ClientAttatchmentDto> GetAll()
        {
            var clientAttatchmentsDtos = new List<ClientAttatchmentDto>();
            try
            {
                var clientAttatchments = _unitOfWork.GenericRepository<ClientAttatchment>().GetAll().ToList();
                clientAttatchmentsDtos = Mapper.Map<List<ClientAttatchment>, List<ClientAttatchmentDto>>(clientAttatchments);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientAttatchmentsDtos;
        }

        public IList<ClientAttatchmentDto> GetForSpecificClient(int clientId)
        {
            var clientAttatchmentsDtos = new List<ClientAttatchmentDto>();
            try
            {
                var clientAttatchments = _unitOfWork.GenericRepository<ClientAttatchment>().FindBy(x => x.ClientId == clientId).ToList();
                clientAttatchmentsDtos = Mapper.Map<List<ClientAttatchment>, List<ClientAttatchmentDto>>(clientAttatchments);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientAttatchmentsDtos;
        }

        public ClientAttatchmentDto GetById(int id)
        {
            ClientAttatchmentDto clientAttatchmentDto = null;
            try
            {
                var clientAttatchment = _unitOfWork.GenericRepository<ClientAttatchment>().GetById(id);
                if (clientAttatchment != null)
                {
                    clientAttatchmentDto = Mapper.Map<ClientAttatchment, ClientAttatchmentDto>(clientAttatchment);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return clientAttatchmentDto;
        }

        public ClientAttatchmentDto Insert(ClientAttatchmentDto dto)
        {
            ClientAttatchmentDto clientAttatchmentDto = null;
            try
            {
                var clientAttatchment = Mapper.Map<ClientAttatchmentDto, ClientAttatchment>(dto);
                clientAttatchment.CreatedBy = _appSession.GetUserName();
                clientAttatchment.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<ClientAttatchment>().Insert(clientAttatchment);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                clientAttatchmentDto = Mapper.Map<ClientAttatchment, ClientAttatchmentDto>(clientAttatchment);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return clientAttatchmentDto;
        }

        public ClientAttatchmentDto Update(ClientAttatchmentDto dto)
        {
            ClientAttatchmentDto clientAttatchmentDto = null;
            try
            {
                var clientAttatchment = _unitOfWork.GenericRepository<ClientAttatchment>().GetById(dto.Id);
                Mapper.Map<ClientAttatchmentDto, ClientAttatchment>(dto, clientAttatchment);
                clientAttatchment.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<ClientAttatchment>().Update(clientAttatchment);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                clientAttatchmentDto = Mapper.Map<ClientAttatchment, ClientAttatchmentDto>(clientAttatchment);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return clientAttatchmentDto;
        }

        public void Delete(int id)
        {
            try
            {
                var clientAttatchment = _unitOfWork.GenericRepository<ClientAttatchment>().GetById(id);
                if (clientAttatchment != null)
                {
                    _unitOfWork.GenericRepository<ClientAttatchment>().Delete(clientAttatchment);
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
