using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BWR.Application.Dtos.Branch;
using BWR.Application.Interfaces.Branch;
using BWR.Domain.Model.Branches;
using BWR.Application.Interfaces.Shared;
using BWR.ShareKernel.Interfaces;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.Domain.Model.Treasures;
using BWR.Domain.Model.Common;

namespace BWR.Application.AppServices.Branch
{
    public class BranchCashAppService : IBranchCashAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public BranchCashAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }
        
        public dynamic GetActualBalance(int coinId, int branchId)
        {
            dynamic treasuryCashsTotal = 0;

            var branchCash = _unitOfWork.GenericRepository<BranchCash>()
                .FindBy(x => x.CoinId == coinId && x.BranchId == branchId).FirstOrDefault();

            
            var treasuryCashs = _unitOfWork.GenericRepository<TreasuryCash>()
                .FindBy(c => c.CoinId == coinId);
            if (treasuryCashs.Any())
                treasuryCashsTotal = treasuryCashs.Sum(x => x.Total);

            return treasuryCashsTotal;

        }

        public IList<BranchCashDto> GetAll()
        {
            var branchCashesDto = new List<BranchCashDto>();
            try
            {
                var branchCashes = _unitOfWork.GenericRepository<BranchCash>().GetAll().ToList();
                branchCashesDto = Mapper.Map<List<BranchCash>, List<BranchCashDto>>(branchCashes);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return branchCashesDto;
        }

        public BranchCashUpdateDto GetForEdit(int id)
        {
            BranchCashUpdateDto branchCashDto = null;
            try
            {
                var branchCash = _unitOfWork.GenericRepository<BranchCash>().GetById(id);
                if (branchCash != null)
                {
                    branchCashDto = Mapper.Map<BranchCash, BranchCashUpdateDto>(branchCash);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return branchCashDto;
        }

        public IList<BranchCashDto> GetForSpecificBranch(int branchId)
        {
            var branchCashesDto = new List<BranchCashDto>();
            try
            {
                var branchCashes = _unitOfWork.GenericRepository<BranchCash>().FindBy(x => x.BranchId == branchId).ToList();
                branchCashesDto = Mapper.Map<List<BranchCash>, List<BranchCashDto>>(branchCashes);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return branchCashesDto;
        }

        public BranchCashDto Insert(BranchCashInsertDto dto)
        {
            BranchCashDto branchCashDto = null;
            try
            {
                var branchCash = Mapper.Map<BranchCashInsertDto, BranchCash>(dto);
                branchCash.CreatedBy = _appSession.GetUserName();
                branchCash.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<BranchCash>().Insert(branchCash);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                branchCashDto = Mapper.Map<BranchCash, BranchCashDto>(branchCash);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return branchCashDto;
        }

        public BranchCashDto Update(BranchCashUpdateDto dto)
        {
            BranchCashDto branchCashDto = null;
            try
            {
                _unitOfWork.CreateTransaction();

                var branchCash = _unitOfWork.GenericRepository<BranchCash>().GetById(dto.Id);
                if (branchCash != null)
                {
                    branchCash.InitialBalance = dto.InitialBalance;
                    branchCash.Total= dto.Total;
                    branchCash.ModifiedBy = _appSession.GetUserName();
                    if(dto.IsMainCoin == true && branchCash.IsMainCoin == false)
                    {
                        branchCash.IsMainCoin = true;

                        var otherBranchCashsWithMainCoin = _unitOfWork.GenericRepository<BranchCash>().FindBy(x => x.Id != dto.Id && x.IsMainCoin == true);

                        if (otherBranchCashsWithMainCoin.Any())
                        {
                            foreach (var otherBranchCashWithMainCoin in otherBranchCashsWithMainCoin)
                            {
                                otherBranchCashWithMainCoin.IsMainCoin = false;
                            }
                        }
                    }
                    else if (dto.IsMainCoin == false && branchCash.IsMainCoin == true)
                    {
                        branchCash.IsMainCoin = false;
                    }
                }
               
                _unitOfWork.GenericRepository<BranchCash>().Update(branchCash);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                branchCashDto = Mapper.Map<BranchCash, BranchCashDto>(branchCash);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return branchCashDto;
        }

        public void UpdateAll(IList<BranchCashUpdateDto> branchCashes)
        {
            _unitOfWork.CreateTransaction();

            foreach (var branchCashDto in branchCashes)
            {
                var branchCash = _unitOfWork.GenericRepository<BranchCash>().FindBy(x => x.CoinId == branchCashDto.CoinId).FirstOrDefault();
                if (branchCash != null)
                {
                    branchCash.ExchangePrice = branchCashDto.ExchangePrice;
                    branchCash.SellingPrice = branchCashDto.SellingPrice;
                    branchCash.PurchasingPrice = branchCashDto.PurchasingPrice;
                    branchCash.ModifiedBy = _appSession.GetUserName();
                    _unitOfWork.Save();
                }
            }
            _unitOfWork.Commit();
        }

        
    }
}
