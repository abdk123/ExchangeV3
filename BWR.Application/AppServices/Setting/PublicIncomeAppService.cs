using AutoMapper;
using BWR.Application.Dtos.Setting.PublicIncome;
using BWR.Application.Interfaces.Setting;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Settings;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BWR.Application.AppServices.Setting
{
    public class PublicIncomeAppService : IPublicIncomeAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public PublicIncomeAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<PublicIncomeDto> GetAll()
        {
            var publicIncomesDtos = new List<PublicIncomeDto>();
            try
            {
                var publicIncomes = _unitOfWork.GenericRepository<PublicIncome>().GetAll().ToList();
                publicIncomesDtos = Mapper.Map<List<PublicIncome>, List<PublicIncomeDto>>(publicIncomes);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return publicIncomesDtos;
        }

        public PublicIncomeDto GetById(int id)
        {
            PublicIncomeDto publicIncomeDto = null;
            try
            {
                var publicIncome = _unitOfWork.GenericRepository<PublicIncome>().GetById(id);
                if (publicIncome != null)
                {
                    publicIncomeDto = Mapper.Map<PublicIncome, PublicIncomeDto>(publicIncome);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return publicIncomeDto;
        }

        public IList<PublicIncomeForDropdownDto> GetForDropdown(string name)
        {
            var publicIncomesDtos = new List<PublicIncomeForDropdownDto>();
            try
            {
                var publicIncomes = _unitOfWork.GenericRepository<PublicIncome>().FindBy(x => x.Name.StartsWith(name)).ToList();
                Mapper.Map<List<PublicIncome>, List<PublicIncomeForDropdownDto>>(publicIncomes, publicIncomesDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return publicIncomesDtos;
        }

        public PublicIncomeUpdateDto GetForEdit(int id)
        {
            PublicIncomeUpdateDto publicIncomeDto = null;
            try
            {
                var publicIncome = _unitOfWork.GenericRepository<PublicIncome>().GetById(id);
                if (publicIncome != null)
                {
                    publicIncomeDto = Mapper.Map<PublicIncome, PublicIncomeUpdateDto>(publicIncome);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return publicIncomeDto;
        }

        public PublicIncomeDto Insert(PublicIncomeInsertDto dto)
        {
            PublicIncomeDto publicIncomeDto = null;
            try
            {
                var publicIncome = Mapper.Map<PublicIncomeInsertDto, PublicIncome>(dto);

                _unitOfWork.CreateTransaction();

                publicIncome.CreatedBy = _appSession.GetUserName();
                publicIncome.IsEnabled = true;

                _unitOfWork.GenericRepository<PublicIncome>().Insert(publicIncome);

                var publicMoney = new PublicMoney()
                {
                    CreatedBy = _appSession.GetUserName(),
                    PublicIncome = publicIncome
                };

                _unitOfWork.GenericRepository<PublicMoney>().Insert(publicMoney);

                _unitOfWork.Save();

                _unitOfWork.Commit();

                publicIncomeDto = Mapper.Map<PublicIncome, PublicIncomeDto>(publicIncome);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return publicIncomeDto;
        }

        public PublicIncomeDto Update(PublicIncomeUpdateDto dto)
        {
            PublicIncomeDto publicIncomeDto = null;
            try
            {
                var publicIncome = _unitOfWork.GenericRepository<PublicIncome>().GetById(dto.Id);
                Mapper.Map<PublicIncomeUpdateDto, PublicIncome>(dto, publicIncome);
                publicIncome.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<PublicIncome>().Update(publicIncome);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                publicIncomeDto = Mapper.Map<PublicIncome, PublicIncomeDto>(publicIncome);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return publicIncomeDto;
        }

        public void Delete(int id)
        {
            try
            {
                var publicIncome = _unitOfWork.GenericRepository<PublicIncome>().GetById(id);
                if (publicIncome != null)
                {
                    _unitOfWork.GenericRepository<PublicIncome>().Delete(publicIncome);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        public bool CheckIfExist(string name, int id)
        {
            try
            {
                var publicIncome = _unitOfWork.GenericRepository<PublicIncome>()
                    .FindBy(x => x.Name.Trim().Equals(name.Trim()) && x.Id != id)
                    .FirstOrDefault();
                if (publicIncome != null)
                    return true;
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return false;
        }
    }
}

