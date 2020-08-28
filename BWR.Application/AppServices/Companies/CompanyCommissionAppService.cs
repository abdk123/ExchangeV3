using AutoMapper;
using BWR.Application.Dtos.Company.CompanyCommission;
using BWR.Application.Interfaces.Company;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Companies;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BWR.Application.AppServices.Companies
{
    public class CompanyCommissionAppService : ICompanyCommissionAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public CompanyCommissionAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }


        public CompanyCommissionsDto GetById(int id)
        {
            CompanyCommissionsDto companyCommissionDto = null;
            try
            {
                var companyCommission = _unitOfWork.GenericRepository<CompanyCommission>().GetById(id);
                if (companyCommission != null)
                {
                    companyCommissionDto = Mapper.Map<CompanyCommission, CompanyCommissionsDto>(companyCommission);
                    companyCommissionDto.CoinName = companyCommission.Coin.Name;
                    companyCommissionDto.CountryName = companyCommission.CompanyCountry.Country.Name;
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companyCommissionDto;
        }

        public IList<CompanyCommissionsDto> GetByCompanyId(int companyId)
        {
            var companyCommissionsDto = new List<CompanyCommissionsDto>();
            try
            {
                var companyCommissions = _unitOfWork.GenericRepository<CompanyCommission>().FindBy(x=>x.CompanyCountry.CompanyId==companyId).ToList();
                companyCommissionsDto = Mapper.Map<List<CompanyCommission>, List<CompanyCommissionsDto>>(companyCommissions);

                companyCommissionsDto.ForEach(x => 
                {
                    var companyCommission = companyCommissions.FirstOrDefault(y => y.Id == x.Id);
                    if (companyCommission != null)
                    {
                        x.CoinName = companyCommission.Coin.Name;
                        x.CountryName = companyCommission.CompanyCountry.Country.Name;
                    }
                });
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companyCommissionsDto;
        }

        public CompanyCommissionUpdateDto GetForEdit(int id)
        {
            CompanyCommissionUpdateDto companyCommissionDto = null;
            try
            {
                var companyCommission = _unitOfWork.GenericRepository<CompanyCommission>().GetById(id);
                if (companyCommission != null)
                {
                    companyCommissionDto = Mapper.Map<CompanyCommission, CompanyCommissionUpdateDto>(companyCommission);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companyCommissionDto;
        }

        public CompanyCommissionDto Insert(CompanyCommissionInsertDto dto)
        {
            CompanyCommissionDto companyCommissionDto = null;
            try
            {
                var companyCommission = Mapper.Map<CompanyCommissionInsertDto, CompanyCommission>(dto);
                companyCommission.CreatedBy = _appSession.GetUserName();
                companyCommission.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<CompanyCommission>().Insert(companyCommission);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                companyCommissionDto = Mapper.Map<CompanyCommission, CompanyCommissionDto>(companyCommission);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return companyCommissionDto;
        }

        public CompanyCommissionDto Update(CompanyCommissionUpdateDto dto)
        {
            CompanyCommissionDto companyCommissionDto = null;
            try
            {
                var companyCommission = _unitOfWork.GenericRepository<CompanyCommission>().GetById(dto.Id);
                Mapper.Map<CompanyCommissionUpdateDto, CompanyCommission>(dto, companyCommission);
                companyCommission.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<CompanyCommission>().Update(companyCommission);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                companyCommissionDto = Mapper.Map<CompanyCommission, CompanyCommissionDto>(companyCommission);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return companyCommissionDto;
        }

        public void Delete(int id)
        {
            try
            {
                var companyCommission = _unitOfWork.GenericRepository<CompanyCommission>().GetById(id);
                if (companyCommission != null)
                {
                    _unitOfWork.GenericRepository<CompanyCommission>().Delete(companyCommission);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        public bool CheckIfExist(int id, int companyCountryId, int coinId)
        {
            return _unitOfWork.GenericRepository<CompanyCommission>()
                .FindBy(x => x.CompanyCountryId.Equals(companyCountryId) && x.CoinId.Equals(coinId) && id!=x.Id)
                .Any();
        }
    }
}
