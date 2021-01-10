using AutoMapper;
using BWR.Application.Dtos.Treasury;
using BWR.Application.Interfaces.Treasury;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Treasures;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using BWR.Application.Dtos.Setting.Coin;

namespace BWR.Application.AppServices.Treasuries
{
    public class TreasuryCashAppService : ITreasuryCashAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public TreasuryCashAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<TreasuryCashDto> GetTreasuryCashes(int treasuryId)
        {
            var treasuryCashsDto = new List<TreasuryCashDto>();
            try
            {
                //var treasuryCashes = _unitOfWork.GenericRepository<TreasuryCash>()
                //    .FindBy(x => x.TreasuryId == treasuryId).ToList();
                //treasuryCashsDto = Mapper.Map<List<TreasuryCash>, List<TreasuryCashDto>>(treasuryCashes);
                var treasuryMoneyAction = _unitOfWork.GenericRepository<TreasuryMoneyAction>()
                    .FindBy(c => c.TreasuryId == treasuryId, "BranchCashFlow", "Coin").ToList();
                var group = treasuryMoneyAction.GroupBy(c => c.Coin);
                foreach (var item in group)
                {
                    
                    var moenyActions = item.ToList();
                    decimal amount = 0;
                    for (int i = 0; i < moenyActions.Count(); i++)
                    {
                        amount += moenyActions[i].RealAmount;
                    }
                    treasuryCashsDto.Add(new TreasuryCashDto()
                    {
                        Coin = Mapper.Map<Domain.Model.Settings.Coin, CoinDto>(item.Key),
                        Amount = amount,
                        CoinId = item.Key.Id,                         
                    });
                }
                //foreach (var item in treasuryCashes)
                //{
                //    treasuryCashsDto.Add(new TreasuryCashDto()
                //    {
                        
                //    })
                //}

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return treasuryCashsDto;
        }

        public TreasuryCashDto Insert(TreasuryCashDto dto)
        {
            TreasuryCashDto treasuryCashDto = null;
            try
            {
                var treasuryCash = Mapper.Map<TreasuryCashDto, TreasuryCash>(dto);
                treasuryCash.CreatedBy = _appSession.GetUserName();
                treasuryCash.IsEnabled = true;
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<TreasuryCash>().Insert(treasuryCash);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                treasuryCashDto = Mapper.Map<TreasuryCash, TreasuryCashDto>(treasuryCash);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return treasuryCashDto;
        }

        public Task<TreasuryCashDto> InsertAsync(TreasuryCashDto dto)
        {
            return Task.Factory.StartNew(()=> {
                TreasuryCashDto treasuryCashDto = null;
                try
                {
                    var treasuryCash = Mapper.Map<TreasuryCashDto, TreasuryCash>(dto);
                    treasuryCash.CreatedBy = _appSession.GetUserName();
                    treasuryCash.IsEnabled = true;
                    _unitOfWork.CreateTransaction();

                    _unitOfWork.GenericRepository<TreasuryCash>().Insert(treasuryCash);
                    _unitOfWork.Save();

                    _unitOfWork.Commit();

                    treasuryCashDto = Mapper.Map<TreasuryCash, TreasuryCashDto>(treasuryCash);
                }
                catch (Exception ex)
                {
                    Tracing.SaveException(ex);
                    _unitOfWork.Rollback();
                }
                return treasuryCashDto;
            });
        }

        public TreasuryCashDto Update(TreasuryCashDto dto)
        {
            TreasuryCashDto treasuryCashDto = null;
            try
            {
                var treasuryCash = _unitOfWork.GenericRepository<TreasuryCash>().GetById(dto.Id);
                Mapper.Map<TreasuryCashDto, TreasuryCash>(dto, treasuryCash);
                treasuryCash.ModifiedBy = _appSession.GetUserName();
                //_unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<TreasuryCash>().Update(treasuryCash);
                _unitOfWork.Save();

                //_unitOfWork.Commit();

                treasuryCashDto = Mapper.Map<TreasuryCash, TreasuryCashDto>(treasuryCash);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return treasuryCashDto;
        }
    }
}
