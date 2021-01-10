using AutoMapper;
using BWR.Application.Dtos.Treasury.TreasuryMoneyAction;
using BWR.Application.Extensions;
using BWR.Application.Interfaces.Common;
using BWR.Application.Interfaces.Shared;
using BWR.Application.Interfaces.TreasuryMoneyAction;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Enums;
using BWR.Domain.Model.Treasures;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;



namespace BWR.Application.AppServices.Treasuries
{
    public class TreasuryMoneyActionAppService : ITreasuryMoneyActionAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IMoneyActionAppService _moneyActionAppService;
        private readonly IAppSession _appSession;

        public TreasuryMoneyActionAppService(
            IUnitOfWork<MainContext> unitOfWork,
            IMoneyActionAppService moneyActionAppService,
            IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _moneyActionAppService = moneyActionAppService;
            _appSession = appSession;
        }

        public IList<TreasuryMoneyActionDto> Get(TreasuryMoneyActionInputDto input)
        {
            var treasuryMoneyActionsDto = new List<TreasuryMoneyActionDto>();
            try
            {
                var treasuryMoneyActions = _unitOfWork.GenericRepository<TreasuryMoneyAction>()
                    .FindBy(x => x.TreasuryId == input.TreasuryId && x.CoinId == input.CoinId, "BranchCashFlow.MoenyAction");


                decimal privousRecoredTotal = 0;
                if (input.From != null)
                {
                    treasuryMoneyActions = treasuryMoneyActions.Where(x => x.Created >= input.From);
                    privousRecoredTotal = treasuryMoneyActions.Sum(c => c.RealAmount);
                }
                if (input.To != null)
                    treasuryMoneyActions = treasuryMoneyActions.Where(x => x.Created <= input.To);
                var ordered = treasuryMoneyActions.OrderBy(c => c.BranchCashFlow?.MoenyAction.Date ?? c.Created).ToList();
                treasuryMoneyActionsDto = (from o in ordered
                                           select new TreasuryMoneyActionDto()
                                           {
                                               Total  = (privousRecoredTotal+=o.RealAmount),
                                               Amount = o.RealAmount,
                                               BranchCashFlowId = o.BranchCashFlowId,
                                               CoinId = o.CoinId,
                                               Created = o.RealDate?.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")) ?? "",
                                               TreasuryId = o.TreasuryId,
                                               Id = o.Id,
                                           }).ToList();
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return treasuryMoneyActionsDto;
        }

        public IList<TreasuryActionsDto> GetMoneyActions(TreasuryMoneyActionInputDto input)
        {
            var treasuryMoneyActionsDto = new List<TreasuryActionsDto>();
            try
            {
                var treasuryMoneyActions = _unitOfWork.GenericRepository<TreasuryMoneyAction>()
                    .FindBy(x => x.TreasuryId == input.TreasuryId && x.CoinId == input.CoinId, c => c.BranchCashFlow.MoenyAction);

                decimal total = 0;
                //if (input.From == null && input.To != null)
                //{
                //    treasuryMoneyActions = treasuryMoneyActions.Where(x => x.Created < input.To);
                //}
                //else if (input.From != null && input.To == null)
                //{
                //    treasuryMoneyActions = treasuryMoneyActions.Where(x => x.Created > input.From);
                //}
                //else if (input.From != null && input.To != null)
                //{
                //    treasuryMoneyActions = treasuryMoneyActions.Where(x => x.Created > input.From && x.Created < input.To);
                //}
                if (input.From != null)
                {
                    treasuryMoneyActions = treasuryMoneyActions.Where(x => x.Created >= input.From);
                    total = treasuryMoneyActions.Where(c => c.Created < input.From).ToList().Sum(c => c.RealAmount);
                }
                foreach (var treasuryMoneyAction in treasuryMoneyActions.ToList())
                {
                    if (treasuryMoneyAction.BranchCashFlowId != null)
                    {

                        var moneyAction = treasuryMoneyAction.BranchCashFlow.MoenyAction;
                        total += treasuryMoneyAction.BranchCashFlow.Amount;
                        treasuryMoneyActionsDto.Add(new TreasuryActionsDto()
                        {
                            //Amount = treasuryMoneyAction.Amount,
                            Amount = treasuryMoneyAction.BranchCashFlow.Amount,
                            //Total = treasuryMoneyAction.Total,
                            Total = total,
                            Id = treasuryMoneyAction.Id,
                            Type = moneyAction.GetTypeName(Requester.Branch, null),
                            Name = _moneyActionAppService.GetActionName(moneyAction),
                            Number = moneyAction.GetActionId(),
                            Date = moneyAction.Date,
                            Note = moneyAction.GetNote(Requester.Branch, null),
                            MoneyActionId = moneyAction.Id,
                            CreatedBy = treasuryMoneyAction.BranchCashFlow.CreatedBy
                        });
                    }
                    else
                    {
                        total += (decimal)treasuryMoneyAction.Amount;
                        treasuryMoneyActionsDto.Add(new TreasuryActionsDto()
                        {
                            Amount = treasuryMoneyAction.Amount ?? 0,
                            Total = total,
                            //Total = treasuryMoneyAction.Total,
                            Id = treasuryMoneyAction.Id,
                            Type = treasuryMoneyAction.Amount > 0 ? "إعطاء" : "اخذ",
                            //Date = treasuryMoneyAction.Created != null ? treasuryMoneyAction.Created.Value.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")) : string.Empty,
                            Date = treasuryMoneyAction.Created.Value,
                            CreatedBy = treasuryMoneyAction.CreatedBy
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return treasuryMoneyActionsDto;
        }


        public TreasuryMoneyActionDto GetMony(TreasuryMoneyActionInsertDto input)
        {
            //هي كلها فيها خطأ برجع بكتبها بعدين
            TreasuryMoneyActionDto treasuryMoneyActionDto = null;
            //try
            //{
            //    decimal total = 0;

            //    _unitOfWork.CreateTransaction();

            //    var treasuryCash = _unitOfWork.GenericRepository<TreasuryCash>()
            //        .FindBy(x => x.CoinId == input.CoinId && x.TreasuryId == input.TreasuryId)
            //        .FirstOrDefault();

            //    if (treasuryCash != null)
            //    {
            //        treasuryCash.Total -= input.Amount;
            //        _unitOfWork.GenericRepository<TreasuryCash>().Update(treasuryCash);
            //        total = treasuryCash.Total;
            //    }
            //    else
            //    {
            //        var newTreasuryCash = new TreasuryCash()
            //        {
            //            CoinId = input.CoinId,
            //            TreasuryId = input.TreasuryId,
            //            Total = input.Amount,
            //            CreatedBy = _appSession.GetUserName(),
            //            Created = DateTime.Now
            //        };
            //        _unitOfWork.GenericRepository<TreasuryCash>().Insert(newTreasuryCash);
            //        total = newTreasuryCash.Total;
            //    }

            //    var treasuryMoneyAction = new TreasuryMoneyAction()
            //    {
            //        //Total = total,
            //        TreasuryId = input.TreasuryId,
            //        CoinId = input.CoinId,
            //        Amount = -input.Amount,
            //        Created = DateTime.Now,
            //        CreatedBy = _appSession.GetUserName()
            //    };
            //    _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

            //    _unitOfWork.Save();

            //    _unitOfWork.Commit();

            //    treasuryMoneyActionDto = Mapper.Map<TreasuryMoneyAction, TreasuryMoneyActionDto>(treasuryMoneyAction);

            //}
            //catch (Exception ex)
            //{
            //    Tracing.SaveException(ex);
            //}
            return treasuryMoneyActionDto;
        }

        public TreasuryMoneyActionDto GiveMony(TreasuryMoneyActionInsertDto input)
        {
            TreasuryMoneyActionDto treasuryMoneyActionDto = null;
            try
            {
                //decimal total = 0;

                _unitOfWork.CreateTransaction();

                var treasuryCash = _unitOfWork.GenericRepository<TreasuryCash>()
                    .FindBy(x => x.CoinId == input.CoinId && x.TreasuryId == input.TreasuryId)
                    .FirstOrDefault();

                if (treasuryCash != null)
                {
                    //treasuryCash.Total += input.Amount;
                    //_unitOfWork.GenericRepository<TreasuryCash>().Update(treasuryCash);
                    //total = treasuryCash.Total;
                }
                else
                {
                    var newTreasuryCash = new TreasuryCash()
                    {
                        CoinId = input.CoinId,
                        TreasuryId = input.TreasuryId,
                        //Total=input.Amount,
                        CreatedBy = _appSession.GetUserName(),
                        Created = DateTime.Now
                    };
                    _unitOfWork.GenericRepository<TreasuryCash>().Insert(newTreasuryCash);
                    //total = newTreasuryCash.Total;
                }

                var treasuryMoneyAction = new TreasuryMoneyAction()
                {
                    //Total = total,
                    TreasuryId = input.TreasuryId,
                    CoinId = input.CoinId,
                    Amount = input.Amount,
                    Created = DateTime.Now,
                    CreatedBy = _appSession.GetUserName()
                };
                _unitOfWork.GenericRepository<TreasuryMoneyAction>().Insert(treasuryMoneyAction);

                _unitOfWork.Save();

                _unitOfWork.Commit();

                treasuryMoneyActionDto = Mapper.Map<TreasuryMoneyAction, TreasuryMoneyActionDto>(treasuryMoneyAction);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return treasuryMoneyActionDto;
        }
    }
}
