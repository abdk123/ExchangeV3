using AutoMapper;
using BWR.Application.Dtos.Setting.Country;
using BWR.Application.Dtos.Setting.Provinces;
using BWR.Application.Interfaces;
using BWR.Application.Interfaces.Setting;
using BWR.Application.Interfaces.Shared;
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
    public class CountryAppService: ICountryAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IProvinceAppService _provinceAppService;
        private readonly IAppSession _appSession;

        public CountryAppService(
            IUnitOfWork<MainContext> unitOfWork,
            IProvinceAppService provinceAppService,
            IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _provinceAppService = provinceAppService;
            _appSession = appSession;
        }
        
        public IList<CountryDto> GetAll()
        {
            var countriesDtos = new List<CountryDto>();
            try
            {
                var countries = _unitOfWork.GenericRepository<Country>().GetAll().Where(x => x.MainCountryId == null).ToList();
                countriesDtos = Mapper.Map<List<Country>, List<CountryDto>>(countries);
            }
            catch(Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return countriesDtos;
        }

        public CountryDto GetById(int id)
        {
            CountryDto countryDto = null;
            try
            {
                var country = _unitOfWork.GenericRepository<Country>().GetById(id);
                if (country != null)
                {
                    countryDto = Mapper.Map<Country, CountryDto>(country);
                }
                
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return countryDto;
        }

        public IList<CountryForDropdownDto> GetForDropdown(string name)
        {
            var countriesDtos = new List<CountryForDropdownDto>();
            try
            {
                var countries = _unitOfWork.GenericRepository<Country>().FindBy(x => x.Name.StartsWith(name) && x.MainCountryId == null).ToList();
                Mapper.Map<List<Country>, List<CountryForDropdownDto>>(countries, countriesDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return countriesDtos;
        }

        public IList<CountryForDropdownDto> GetCountriesAndProvinces()
        {
            var countriesDtos = new List<CountryForDropdownDto>();
            try
            {
                var countries = _unitOfWork.GenericRepository<Country>().GetAll().ToList();
                Mapper.Map<List<Country>, List<CountryForDropdownDto>>(countries, countriesDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return countriesDtos;
        }

        public CountryDto Insert(CountryInsertDto dto)
        {
            CountryDto countryDto = null;
            try
            {
                var country = Mapper.Map<CountryInsertDto, Country>(dto);
                country.CreatedBy = _appSession.GetUserName();
                country.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Country>().Insert(country);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                countryDto = Mapper.Map<Country, CountryDto>(country);
            }
            catch(Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return countryDto;
        }

        public CountryDto Update(CountryUpdateDto dto)
        {
            CountryDto countryDto = null;
            try
            {
                var country = _unitOfWork.GenericRepository<Country>().GetById(dto.Id);
                Mapper.Map<CountryUpdateDto, Country>(dto, country);
                country.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Country>().Update(country);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                CheckForDelete(dto.Provinces, country.Provinces);
                CheckForAdd(dto.Provinces, country.Id);

                countryDto = Mapper.Map<Country, CountryDto>(country);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return countryDto;
        }

        public void Delete(int id)
        {
            try
            {
                _unitOfWork.CreateTransaction();
                var country = _unitOfWork.GenericRepository<Country>().GetById(id);
                if (country != null)
                {
                    var provinces = _unitOfWork.GenericRepository<Country>().FindBy(x => x.MainCountryId == id).ToList();

                    _unitOfWork.GenericRepository<Country>().Delete(country);
                    _unitOfWork.Save();
                    _unitOfWork.Commit();

                    if (provinces.Any())
                    {
                        foreach (var province in provinces)
                        {
                            _provinceAppService.Delete(province.Id);
                        }
                    }
                    
                   
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
        }

        public bool CheckIfExist(string name,int id)
        {
            try
            {
                var country = _unitOfWork.GenericRepository<Country>()
                    .FindBy(x => x.Name.Trim().Equals(name.Trim()) && x.MainCountry == null && x.Id != id)
                    .FirstOrDefault();
                if (country != null)
                    return true;
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return false;
        }

        public CountryUpdateDto GetForEdit(int id)
        {
            CountryUpdateDto countryDto = null;
            try
            {
                var country = _unitOfWork.GenericRepository<Country>().GetById(id);
                if (country != null)
                {
                    countryDto = Mapper.Map<Country, CountryUpdateDto>(country);
                }

                

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return countryDto;
        }


        private void CheckForDelete(IList<CountryDto> provincesDtos, IList<Country> provinces)
        {
            try
            {
                var deletedProvinces = provinces.Where(x => !provincesDtos.Select(y => y.Id).Contains(x.Id));
                if (deletedProvinces.Any())
                {
                    foreach (var deletedProvince in deletedProvinces)
                    {
                        _provinceAppService.Delete(deletedProvince.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

        }

        private void CheckForAdd(IList<CountryDto> provincesDtos,int countryId)
        {
            try
            {
                var newprovinces = provincesDtos.Where(x => x.Id == 0);
                if (newprovinces.Any())
                {
                    foreach (var newprovince in newprovinces)
                    {
                        var provinceInsertDto = new ProvinceInsertDto()
                        {
                            Name= newprovince.Name,
                            MainCountryId= countryId,
                            CreatedBy=_appSession.GetUserName()
                        };

                        _provinceAppService.Insert(provinceInsertDto);
                    }
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

        }
    }
}
