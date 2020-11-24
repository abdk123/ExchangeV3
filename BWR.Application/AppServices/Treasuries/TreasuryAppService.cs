using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Treasury;
using BWR.Application.Interfaces.Shared;
using BWR.Application.Interfaces.Treasury;
using BWR.Domain.Model.Treasures;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;

namespace BWR.Application.AppServices.Treasuries
{
    public class TreasuryAppService : ITreasuryAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly ITreasuryCashAppService _treasuryCashAppService;
        private readonly IAppSession _appSession;

        public TreasuryAppService(IUnitOfWork<MainContext> unitOfWork
            , ITreasuryCashAppService treasuryCashAppService
            , IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _treasuryCashAppService = treasuryCashAppService;
            _appSession = appSession;
        }

        public IList<TreasuryDto> GetAll()
        {
            var treasuriesDtos = new List<TreasuryDto>();
            try
            {
                var treasuries = _unitOfWork.GenericRepository<Treasury>().GetAll().ToList();
                treasuriesDtos = Mapper.Map<List<Treasury>, List<TreasuryDto>>(treasuries);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return treasuriesDtos;
        }

        public IList<TreasurysDto> GetAllWithBalances()
        {
            var treasuriesDtos = new List<TreasurysDto>();
            try
            {
                var treasuries = _unitOfWork.GenericRepository<Treasury>().FindBy(c=>c.IsMainTreasury==false).ToList();
                if (treasuries.Any())
                {
                    treasuriesDtos = (from t in treasuries
                                      select new TreasurysDto()
                                      {
                                          Id = t.Id,
                                          IsAvilable = t.IsAvilable,
                                          IsEnabled = t.IsEnabled,
                                          Name = t.Name,            
                                          Balances = GetTreasuryCashesForDto(t.TreasuryCashes),
                                      }).ToList();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return treasuriesDtos;
        }


        public TreasuryDto GetById(int id)
        {
            TreasuryDto treasuryDto = null;
            try
            {
                var treasury = _unitOfWork.GenericRepository<Treasury>().GetById(id);
                if (treasury != null)
                {
                    treasuryDto = Mapper.Map<Treasury, TreasuryDto>(treasury);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return treasuryDto;
        }

        public TreasuryUpdateDto GetForEdit(int id)
        {
            TreasuryUpdateDto treasuryDto = null;
            try
            {
                var treasury = _unitOfWork.GenericRepository<Treasury>().GetById(id);
                if (treasury != null)
                {
                    treasuryDto = Mapper.Map<Treasury, TreasuryUpdateDto>(treasury);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return treasuryDto;
        }

        public TreasuryDto Insert(TreasuryInsertDto dto)
        {
            TreasuryDto treasuryDto = null;
            try
            {
                var treasury = Mapper.Map<TreasuryInsertDto, Treasury>(dto);
                treasury.BranchId = BranchHelper.Id;
                treasury.IsEnabled = true;
                treasury.IsAvilable = true;
                _unitOfWork.CreateTransaction();
                _unitOfWork.GenericRepository<Treasury>().Insert(treasury);
                foreach (var item in treasury.TreasuryCashes)
                {
                    var treasuryMoneyAction = new TreasuryMoneyAction()
                    {
                        Total = item.Total,
                        TreasuryId = treasury.Id,
                        CoinId=  item.CoinId,
                        Amount = item.Total,
                    };
                    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);
                }
                _unitOfWork.Save();

                _unitOfWork.Commit();

                treasuryDto = Mapper.Map<Treasury, TreasuryDto>(treasury);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return treasuryDto;
        }

        public TreasuryDto Update(TreasuryUpdateDto dto)
        {
            TreasuryDto treasuryDto = null;
            try
            {
                var treasury = _unitOfWork.GenericRepository<Treasury>().GetById(dto.Id);

                _unitOfWork.CreateTransaction();

                foreach (var treasuryCashDto in dto.TreasuryCashes)
                {
                    _treasuryCashAppService.Update(treasuryCashDto);
                }


                //treasury.ModifiedBy = _appSession.GetUserName();
                treasury.Name = dto.Name;

                _unitOfWork.GenericRepository<Treasury>().Update(treasury);
                _unitOfWork.Save();

                _unitOfWork.Commit();
                _unitOfWork.GenericRepository<Treasury>().RefershEntity(treasury);
                treasuryDto = Mapper.Map<Treasury, TreasuryDto>(treasury);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return treasuryDto;
        }


        public void Delete(int id)
        {
            try
            {
                var treasury = _unitOfWork.GenericRepository<Treasury>().GetById(id);
                if (treasury != null)
                {
                    _unitOfWork.GenericRepository<Treasury>().Delete(treasury);
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
                var treasury = _unitOfWork.GenericRepository<Treasury>()
                    .FindBy(x => x.Name.Trim().Equals(name.Trim()) && x.Id != id)
                    .FirstOrDefault();
                if (treasury != null)
                    return true;
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return false;
        }


        private string GetTreasuryCashesForDto(IList<TreasuryCash> treasuryCashes)
        {
            var balance = "";
            foreach (var treasuryCash in treasuryCashes)
            {
                var total = treasuryCash.Total;
                var coinName = treasuryCash.Coin != null ? treasuryCash.Coin.Name : string.Empty;
                if (total != 0)
                {
                    balance += $"{total} {coinName} <br /> ";
                }
            }
            return balance;
        }

        public TreasuryDto GetTreasuryForUser(string userName)
        {
            TreasuryDto dto = null;
            try
            {
                var userTreasuries = _unitOfWork.GenericRepository<UserTreasuery>().FindBy(x => x.User.UserName == userName && x.DeliveryDate == null);
                if (userTreasuries.Any())
                {
                    dto = new TreasuryDto()
                    {
                        Id = userTreasuries.LastOrDefault().Id
                    };

                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return dto;
        }
    }

}
