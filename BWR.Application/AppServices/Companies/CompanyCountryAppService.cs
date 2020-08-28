using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BWR.Application.Common;
using BWR.Application.Dtos.Company;
using BWR.Application.Interfaces.Company;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Settings;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;

namespace BWR.Application.AppServices.Companies
{
    public class CompanyCountryAppService : ICompanyCountryAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public CompanyCountryAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public CompanyCountryDto Insert(CompanyCountryDto dto)
        {
            CompanyCountryDto companyCountryDto = null;
            try
            {
                var companyCountry = Mapper.Map<CompanyCountryDto, CompanyCountry>(dto);
                companyCountry.CreatedBy = _appSession.GetUserName();
                companyCountry.IsEnabled = true;

                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<CompanyCountry>().Insert(companyCountry);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                companyCountryDto = Mapper.Map<CompanyCountry, CompanyCountryDto>(companyCountry);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return companyCountryDto;
        }

        public CompanyCountryDto Update(CompanyCountryDto dto)
        {
            CompanyCountryDto companyCountryDto = null;
            try
            {
                var companyCountry = _unitOfWork.GenericRepository<CompanyCountry>().GetById(dto.Id);
                Mapper.Map<CompanyCountryDto, CompanyCountry>(dto, companyCountry);
                companyCountry.ModifiedBy = _appSession.GetUserName();
                //_unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<CompanyCountry>().Update(companyCountry);
                _unitOfWork.Save();

                //_unitOfWork.Commit();

                companyCountryDto = Mapper.Map<CompanyCountry, CompanyCountryDto>(companyCountry);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                //_unitOfWork.Rollback();
            }
            return companyCountryDto;
        }

        public void Delete(int id)
        {
            try
            {
                var companyCountry = _unitOfWork.GenericRepository<CompanyCountry>().GetById(id);
                if (companyCountry != null)
                {
                    _unitOfWork.GenericRepository<CompanyCountry>().Delete(companyCountry);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        public IList<CompanyCountriesDto> GetCountriesForCompany(int companyId)
        {
            IList<CompanyCountriesDto> companyDtos = new List<CompanyCountriesDto>();

            try
            {
                var companyCountries = _unitOfWork.GenericRepository<CompanyCountry>().FindBy(x => x.CompanyId == companyId).ToList();
                foreach (var companyCountry in companyCountries)
                {
                    var companyDto = new CompanyCountriesDto()
                    {
                        Id= companyCountry.Id,
                        CompanyId =companyId,
                        CountryId=companyCountry.CountryId,
                        CountryName= companyCountry.Country!=null ? companyCountry.Country.Name : _unitOfWork.GenericRepository<Country>().GetById(companyCountry.CountryId).Name,
                        IsEnabled=companyCountry.IsEnabled
                    };

                    companyDtos.Add(companyDto);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return companyDtos;
        }

        public bool CheckIfExist(int companyId, int countryId)
        {
            return _unitOfWork.GenericRepository<CompanyCountry>().FindBy(x => x.CompanyId == companyId && x.CountryId == countryId).Any();
        }

        public IList<DtoForDropdown> GetCompanyCountriesForDropdown(int companyId)
        {
            var dtosForDropdown = new List<DtoForDropdown>();
            try
            {
                var companyCountries = _unitOfWork.GenericRepository<CompanyCountry>().FindBy(x => x.CompanyId == companyId).ToList();

                dtosForDropdown = (from c in companyCountries
                                       select new DtoForDropdown
                                       {
                                           Id = c.Id,
                                           Name = c.Country != null ? c.Country.Name : string.Empty
                                       }).ToList();
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return dtosForDropdown;
        }

        
    }
}
