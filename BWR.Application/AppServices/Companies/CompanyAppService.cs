using AutoMapper;
using BWR.Application.Dtos.Company;
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
    public class CompanyAppService : ICompanyAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly ICompanyCashAppService _companyCashAppService;
        private readonly ICompanyCountryAppService _companyCountryAppService;
        private readonly IAppSession _appSession;

        public CompanyAppService(IUnitOfWork<MainContext> unitOfWork
            , ICompanyCashAppService companyCashAppService
            , ICompanyCountryAppService companyCountryAppService
            , IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _companyCashAppService = companyCashAppService;
            _companyCountryAppService = companyCountryAppService;
            _appSession = appSession;
        }

        public IList<CompanyDto> GetAll()
        {
            var companiesDtos = new List<CompanyDto>();
            try
            {
                var companies = _unitOfWork.GenericRepository<Company>().GetAll().ToList();
                companiesDtos = Mapper.Map<List<Company>, List<CompanyDto>>(companies);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companiesDtos;
        }

        public CompanyDto GetById(int id)
        {
            CompanyDto companyDto = null;
            try
            {
                var company = _unitOfWork.GenericRepository<Company>().GetById(id);
                if (company != null)
                {
                    companyDto = Mapper.Map<Company, CompanyDto>(company);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companyDto;
        }

        public IList<CompanyForDropdownDto> GetForDropdown(string name)
        {
            var companiesDtos = new List<CompanyForDropdownDto>();
            try
            {
                var companies = _unitOfWork.GenericRepository<Company>().FindBy(x => x.Name.StartsWith(name)).ToList();
                Mapper.Map<List<Company>, List<CompanyForDropdownDto>>(companies, companiesDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companiesDtos;
        }

        public CompanyUpdateDto GetForEdit(int id)
        {
            CompanyUpdateDto companyDto = null;
            try
            {
                var company = _unitOfWork.GenericRepository<Company>().GetById(id);
                if (company != null)
                {
                    companyDto = Mapper.Map<Company, CompanyUpdateDto>(company);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companyDto;
        }

        public CompanyDto Insert(CompanyInsertDto dto)
        {
            CompanyDto companyDto = null;
            try
            {
                var company = Mapper.Map<CompanyInsertDto, Company>(dto);
                company.CreatedBy = _appSession.GetUserName();
                company.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Company>().Insert(company);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                companyDto = Mapper.Map<Company, CompanyDto>(company);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return companyDto;
        }

        public CompanyDto Update(CompanyUpdateDto dto)
        {
            CompanyDto companyDto = null;
            try
            {
                var company = _unitOfWork.GenericRepository<Company>().GetById(dto.Id);

                _unitOfWork.CreateTransaction();

                foreach (var companyCashDto in dto.CompanyCashes)
                {
                    if (companyCashDto.Id == null || companyCashDto.Id == 0)
                    {
                        _companyCashAppService.Insert(companyCashDto);
                    }
                    else
                    {
                        _companyCashAppService.Update(companyCashDto);
                    }
                }

                CheckForDelete(dto.CompanyCountries, company.CompanyCountries);
                CheckForAdd(dto.CompanyCountries);

                company.ModifiedBy = _appSession.GetUserName();
                company.Name = dto.Name;

                _unitOfWork.GenericRepository<Company>().Update(company);
                _unitOfWork.Save();

                _unitOfWork.Commit();
                _unitOfWork.GenericRepository<Company>().RefershEntity(company);
                companyDto = Mapper.Map<Company, CompanyDto>(company);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return companyDto;
        }

        private void CheckForDelete(IList<CompanyCountryDto> companyCountriesDtos, IList<CompanyCountry> companyCountries)
        {
            try
            {
                var deletedCountries = companyCountries.Where(x => !companyCountriesDtos.Select(y => y.Id).Contains(x.Id));
                if (deletedCountries.Any())
                {
                    foreach(var deletedCountry in deletedCountries)
                    {
                        _companyCountryAppService.Delete(deletedCountry.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
           
        }

        private void CheckForAdd(IList<CompanyCountryDto> companyCountriesDtos)
        {
            try
            {
                var newCompanyCountries = companyCountriesDtos.Where(x => x.Id == null);
                if (newCompanyCountries.Any())
                {
                    foreach (var newCountry in newCompanyCountries)
                    {
                        _companyCountryAppService.Insert(newCountry);
                    }
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

        }

        public void Delete(int id)
        {
            try
            {
                var company = _unitOfWork.GenericRepository<Company>().GetById(id);
                if (company != null)
                {
                    _unitOfWork.GenericRepository<Company>().Delete(company);
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
                var company = _unitOfWork.GenericRepository<Company>()
                    .FindBy(x => x.Name.Trim().Equals(name.Trim()) && x.Id != id)
                    .FirstOrDefault();
                if (company != null)
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
