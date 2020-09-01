using AutoMapper;
using BWR.Application.Dtos.Setting.PublicExpense;
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
    public class PublicExpenseAppService : IPublicExpenseAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public PublicExpenseAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<PublicExpenseDto> GetAll()
        {
            var publicExpensesDtos = new List<PublicExpenseDto>();
            try
            {
                var publicExpenses = _unitOfWork.GenericRepository<PublicExpense>().GetAll().ToList();
                publicExpensesDtos = Mapper.Map<List<PublicExpense>, List<PublicExpenseDto>>(publicExpenses);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return publicExpensesDtos;
        }

        public PublicExpenseDto GetById(int id)
        {
            PublicExpenseDto publicExpenseDto = null;
            try
            {
                var publicExpense = _unitOfWork.GenericRepository<PublicExpense>().GetById(id);
                if (publicExpense != null)
                {
                    publicExpenseDto = Mapper.Map<PublicExpense, PublicExpenseDto>(publicExpense);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return publicExpenseDto;
        }

        public IList<PublicExpenseForDropdownDto> GetForDropdown(string name)
        {
            var publicExpensesDtos = new List<PublicExpenseForDropdownDto>();
            try
            {
                var publicExpenses = _unitOfWork.GenericRepository<PublicExpense>().FindBy(x => x.Name.StartsWith(name)).ToList();
                Mapper.Map<List<PublicExpense>, List<PublicExpenseForDropdownDto>>(publicExpenses, publicExpensesDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return publicExpensesDtos;
        }

        public PublicExpenseUpdateDto GetForEdit(int id)
        {
            PublicExpenseUpdateDto publicExpenseDto = null;
            try
            {
                var publicExpense = _unitOfWork.GenericRepository<PublicExpense>().GetById(id);
                if (publicExpense != null)
                {
                    publicExpenseDto = Mapper.Map<PublicExpense, PublicExpenseUpdateDto>(publicExpense);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return publicExpenseDto;
        }

        public PublicExpenseDto Insert(PublicExpenseInsertDto dto)
        {
            PublicExpenseDto publicExpenseDto = null;
            try
            {
                var publicExpense = Mapper.Map<PublicExpenseInsertDto, PublicExpense>(dto);

                _unitOfWork.CreateTransaction();

                publicExpense.CreatedBy = _appSession.GetUserName();
                publicExpense.IsEnabled = true;
                
                _unitOfWork.GenericRepository<PublicExpense>().Insert(publicExpense);

                var publicMoney = new PublicMoney()
                {
                    CreatedBy = _appSession.GetUserName(),
                    ExpenseId = publicExpense.Id
                };

                _unitOfWork.GenericRepository<PublicMoney>().Insert(publicMoney);
                _unitOfWork.Save();
                _unitOfWork.Commit();

                publicExpenseDto = Mapper.Map<PublicExpense, PublicExpenseDto>(publicExpense);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return publicExpenseDto;
        }

        public PublicExpenseDto Update(PublicExpenseUpdateDto dto)
        {
            PublicExpenseDto publicExpenseDto = null;
            try
            {
                var publicExpense = _unitOfWork.GenericRepository<PublicExpense>().GetById(dto.Id);
                Mapper.Map<PublicExpenseUpdateDto, PublicExpense>(dto, publicExpense);
                publicExpense.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<PublicExpense>().Update(publicExpense);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                publicExpenseDto = Mapper.Map<PublicExpense, PublicExpenseDto>(publicExpense);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return publicExpenseDto;
        }

        public void Delete(int id)
        {
            try
            {
                var publicExpense = _unitOfWork.GenericRepository<PublicExpense>().GetById(id);
                if (publicExpense != null)
                {
                    _unitOfWork.GenericRepository<PublicExpense>().Delete(publicExpense);
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
                var publicExpense = _unitOfWork.GenericRepository<PublicExpense>()
                    .FindBy(x => x.Name.Trim().Equals(name.Trim()) && x.Id != id)
                    .FirstOrDefault();
                if (publicExpense != null)
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