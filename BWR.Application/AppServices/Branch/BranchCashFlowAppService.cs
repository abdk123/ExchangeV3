using AutoMapper;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Branch.BranchCashFlow;
using BWR.Application.Dtos.BranchCashFlow;
using BWR.Application.Extensions;
using BWR.Application.Interfaces.BranchCashFlow;
using BWR.Application.Interfaces.Common;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Enums;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BWR.Application.AppServices.Branches
{
    public class BranchCashFlowAppService : IBranchCashFlowAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IMoneyActionAppService _moneyActionAppService;
        private readonly IAppSession _appSession;

        public BranchCashFlowAppService(IUnitOfWork<MainContext> unitOfWork,
            IMoneyActionAppService moneyActionAppService,
            IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _moneyActionAppService = moneyActionAppService;
            _appSession = appSession;
        }
        
        public IList<BranchCashFlowDto> GetAll()
        {
            var branchcashflowsDtos = new List<BranchCashFlowDto>();
            try
            {
                var branchcashflows = _unitOfWork.GenericRepository<BranchCashFlow>()
                    .GetAll().OrderBy(x => x.MoenyAction.Date).ToList();
                branchcashflowsDtos = Mapper.Map<List<BranchCashFlow>, List<BranchCashFlowDto>>(branchcashflows);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return branchcashflowsDtos;
        }

        public IList<BranchCashFlowDto> Get(Expression<Func<BranchCashFlow, bool>> predicate)
        {
            var branchcashflowsDtos = new List<BranchCashFlowDto>();
            try
            {
                var branchcashflows = _unitOfWork.GenericRepository<BranchCashFlow>()
                    .FindBy(predicate).OrderBy(x => x.MoenyAction.Date).ToList();
                if (branchcashflows.Any())
                {
                    branchcashflowsDtos = Mapper.Map<List<BranchCashFlow>, List<BranchCashFlowDto>>(branchcashflows);
                }
                
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return branchcashflowsDtos;
        }

        public IList<BranchCashFlowOutputDto> Get(int? branchId, int coinId, DateTime? from, DateTime? to)
        {
            IList<BranchCashFlowOutputDto> branchCashFlowsDto = new List<BranchCashFlowOutputDto>() ;
            try
            {
                if(branchId==null)
                    branchId = BranchHelper.Id;

                #region Last Total

                decimal lastTotal;
                var brachCashs = _unitOfWork.GenericRepository<BranchCash>()
                    .FindBy(c => c.CoinId == coinId && c.BranchId == branchId).ToList();

                if (!brachCashs.Any())
                    return branchCashFlowsDto;

                lastTotal = brachCashs.FirstOrDefault().InitialBalance;
                branchCashFlowsDto.Add(new BranchCashFlowOutputDto
                {
                    Balance = lastTotal,
                    Type = "—’Ìœ ”«»ﬁ",
                });


                #endregion

                var allBranchCashFlows = _unitOfWork.GenericRepository<BranchCashFlow>()
                    .FindBy(c => c.CoinId == coinId && c.BranchId == branchId).OrderBy(x =>new { x.MoenyAction.Date, x.Id }).ToList();
                if (allBranchCashFlows.Any())
                {
                    var branchCashFlows = new List<BranchCashFlow>();

                    if (from != null || to != null)
                    {
                        if (from != null)
                        {
                            var tempLastBranchCahsFlow = allBranchCashFlows.Where(c => c.MoenyAction.Date< from).ToList();
                            if (tempLastBranchCahsFlow.Any())
                            {
                                branchCashFlowsDto.Clear();
                                lastTotal = tempLastBranchCahsFlow.Last().Total;
                                branchCashFlowsDto.Add(new BranchCashFlowOutputDto
                                {
                                    Balance = lastTotal,
                                    Type = "—’Ìœ ”«»ﬁ"                                    
                                });
                            }
                        }

                        if (from != null && to == null)
                            branchCashFlows = allBranchCashFlows.Where(c => c.MoenyAction.Date >= from).ToList();
                        else if (to != null && from == null)
                            branchCashFlows = allBranchCashFlows.Where(c => c.MoenyAction.Date <= to).ToList();
                        else
                            branchCashFlows = allBranchCashFlows.Where(c => c.MoenyAction.Date >= from && c.MoenyAction.Date <= to).ToList();

                    }
                    else
                    {
                        branchCashFlows = allBranchCashFlows.ToList();
                    }

                    foreach (var item in branchCashFlows)
                    {
                        string note = item.MoenyAction.GetNote(Requester.Branch, null);
                        var dto = new BranchCashFlowOutputDto()
                        {
                            Id = item.Id,
                            Balance = branchCashFlowsDto.Last().Balance + item.Amount,
                            Amount = item.Amount,
                            Type = item.MoenyAction.GetTypeName(Requester.Branch, null),
                            Name = _moneyActionAppService.GetActionName(item.MoenyAction),
                            Number = item.MoenyAction.GetActionId().ToString(),
                            Date = item.MoenyAction.GetDate(),
                            Note = note,
                            MoneyActionId = item.MoenyAction.Id,
                            CreatedBy = item.MoenyAction.CreateBy()
                        };
                        branchCashFlowsDto.Add(dto);
                    }

                    
                }
                
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return branchCashFlowsDto;
        }
    }
}
