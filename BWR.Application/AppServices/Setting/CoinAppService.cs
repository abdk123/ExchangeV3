using AutoMapper;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Interfaces.Branch;
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
    public class CoinAppService : ICoinAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IBranchCashAppService _branchCashAppService;
        private readonly IAppSession _appSession;

        public CoinAppService(IUnitOfWork<MainContext> unitOfWork,
            IBranchCashAppService branchCashAppService,
            IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _branchCashAppService = branchCashAppService;
            _appSession = appSession;
        }

        public IList<CoinDto> GetAll()
        {
            var coinsDtos = new List<CoinDto>();
            try
            {
                var coins = _unitOfWork.GenericRepository<Coin>().GetAll().ToList();
                coinsDtos = Mapper.Map<List<Coin>, List<CoinDto>>(coins);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return coinsDtos;
        }

        public CoinDto GetById(int id)
        {
            CoinDto coinDto = null;
            try
            {
                var coin = _unitOfWork.GenericRepository<Coin>().GetById(id);
                if (coin != null)
                {
                    coinDto = Mapper.Map<Coin, CoinDto>(coin);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return coinDto;
        }

        public IList<CoinForDropdownDto> GetForDropdown(string name)
        {
            var coinsDtos = new List<CoinForDropdownDto>();
            try
            {
                var coins = _unitOfWork.GenericRepository<Coin>().FindBy(x => x.Name.StartsWith(name)).ToList();
                Mapper.Map<List<Coin>, List<CoinForDropdownDto>>(coins, coinsDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return coinsDtos;
        }

        public CoinUpdateDto GetForEdit(int id)
        {
            CoinUpdateDto coinDto = null;
            try
            {
                var coin = _unitOfWork.GenericRepository<Coin>().GetById(id);
                if (coin != null)
                {
                    coinDto = Mapper.Map<Coin, CoinUpdateDto>(coin);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return coinDto;
        }

        public CoinDto Insert(CoinInsertDto dto)
        {
            CoinDto coinDto = null;
            try
            {
                var coin = Mapper.Map<CoinInsertDto, Coin>(dto);
                coin.CreatedBy = _appSession.GetUserName();
                coin.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Coin>().Insert(coin);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                coinDto = Mapper.Map<Coin, CoinDto>(coin);

                var branchCashInsertDto = new BranchCashInsertDto()
                {
                    CoinId = coinDto.Id.Value,
                    BranchId = BranchHelper.Id

                };
                _branchCashAppService.Insert(branchCashInsertDto);

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return coinDto;
        }

        public CoinDto Update(CoinUpdateDto dto)
        {
            CoinDto coinDto = null;
            try
            {
                var coin = _unitOfWork.GenericRepository<Coin>().GetById(dto.Id);
                Mapper.Map<CoinUpdateDto, Coin>(dto, coin);
                coin.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<Coin>().Update(coin);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                coinDto = Mapper.Map<Coin, CoinDto>(coin);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return coinDto;
        }

        public void Delete(int id)
        {
            try
            {
                var coin = _unitOfWork.GenericRepository<Coin>().GetById(id);
                if (coin != null)
                {
                    _unitOfWork.GenericRepository<Coin>().Delete(coin);
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
                var coin = _unitOfWork.GenericRepository<Coin>()
                    .FindBy(x => x.Name.Trim().Equals(name.Trim()) && x.Id != id)
                    .FirstOrDefault();
                if (coin != null)
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
