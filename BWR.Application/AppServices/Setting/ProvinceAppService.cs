using AutoMapper;
using BWR.Application.Dtos.Setting.Provinces;
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
    public class ProvinceAppService : IProvinceAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public ProvinceAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<ProvinceDto> GetAll()
        {
            var countriesDtos = new List<ProvinceDto>();
            try
            {
                var countries = _unitOfWork.GenericRepository<Country>().GetAll().Where(x => x.MainCountryId != null).ToList();
                countriesDtos = Mapper.Map<List<Country>, List<ProvinceDto>>(countries);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return countriesDtos;
        }

        public ProvinceDto GetById(int id)
        {
            ProvinceDto provinceDto = null;
            try
            {
                var province = _unitOfWork.GenericRepository<Country>().GetById(id);
                if (province != null)
                {
                    provinceDto = Mapper.Map<Country, ProvinceDto>(province);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return provinceDto;
        }

        public IList<ProvinceForDropdownDto> GetForDropdown(string name)
        {
            var countriesDtos = new List<ProvinceForDropdownDto>();
            try
            {
                var countries = _unitOfWork.GenericRepository<Country>().FindBy(x => x.Name.StartsWith(name) && x.MainCountryId == null).ToList();
                Mapper.Map<List<Country>, List<ProvinceForDropdownDto>>(countries, countriesDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return countriesDtos;
        }

        public ProvinceUpdateDto GetForEdit(int id)
        {
            ProvinceUpdateDto provinceDto = null;
            try
            {
                var province = _unitOfWork.GenericRepository<Country>().GetById(id);
                if (province != null)
                {
                    provinceDto = Mapper.Map<Country, ProvinceUpdateDto>(province);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return provinceDto;
        }

        public IList<ProvinceDto> GetProvinceForSpecificCountry(int countryId)
        {
            var provinceDtos = new List<ProvinceDto>();
            try
            {
                var provinces = _unitOfWork.GenericRepository<Country>().FindBy(x => x.MainCountryId == countryId).ToList();
                if (provinces.Any())
                {
                    Mapper.Map<List<Country>, List<ProvinceDto>>(provinces, provinceDtos);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return provinceDtos;
            
        }

        public ProvinceDto Insert(ProvinceInsertDto dto)
        {
            ProvinceDto provinceDto = null;
            try
            {
                var province = Mapper.Map<ProvinceInsertDto, Country>(dto);
                province.CreatedBy = _appSession.GetUserName();
                province.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Country>().Insert(province);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                provinceDto = Mapper.Map<Country, ProvinceDto>(province);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return provinceDto;
        }

        public ProvinceDto Update(ProvinceUpdateDto dto)
        {
            ProvinceDto provinceDto = null;
            try
            {
                var province = _unitOfWork.GenericRepository<Country>().GetById(dto.Id);
                Mapper.Map<ProvinceUpdateDto, Country>(dto, province);
                province.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Country>().Update(province);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                provinceDto = Mapper.Map<Country, ProvinceDto>(province);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return provinceDto;
        }

        public void Delete(int id)
        {
            try
            {
                var province = _unitOfWork.GenericRepository<Country>().GetById(id);
                if (province != null)
                {
                    _unitOfWork.GenericRepository<Country>().Delete(province);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        public bool CheckIfExist(string name, int countryId ,int id)
        {
            try
            {
                var province = _unitOfWork.GenericRepository<Country>()
                    .FindBy(x => x.Name.Trim().Equals(name.Trim()) && x.MainCountryId == countryId && x.Id != id)
                    .FirstOrDefault();
                if (province != null)
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
