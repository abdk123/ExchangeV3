using AutoMapper;
using BWR.Application.Dtos.Company.CompanyCashFlow;
using BWR.Application.Extensions;
using BWR.Application.Interfaces.Common;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Enums;
using BWR.Domain.Model.Settings;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BWR.Application.Interfaces.Company;
using System;
using BWR.Application.Dtos.Statement;
using BWR.Domain.Model.Security;

namespace BWR.Application.AppServices.Companies
{
    public class CompanyCashFlowAppService : ICompanyCashFlowAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IMoneyActionAppService _moneyActionAppService;
        private readonly IAppSession _appSession;

        public CompanyCashFlowAppService(
            IUnitOfWork<MainContext> unitOfWork,
            IMoneyActionAppService moneyActionAppService,
            IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _moneyActionAppService = moneyActionAppService;
            _appSession = appSession;
        }

        public IList<CompanyCashFlowOutputDto> Get(CompanyCashFlowInputDto input)
        {
            var companyCashFlowsDtos = new List<CompanyCashFlowOutputDto>();
            try
            {
                if (input.CoinId != 0)
                {
                    decimal? lastBalance = 0;
                    var companyCash = _unitOfWork.GenericRepository<CompanyCash>()
                        .FindBy(x=>x.CoinId == input.CoinId && x.CompanyId == input.CompanyId).FirstOrDefault();
                    if (companyCash != null)
                    {
                        lastBalance = companyCash.InitialBalance;
                    }
                    var companyCashFlows = _unitOfWork.GenericRepository<CompanyCashFlow>()
                        .FindBy(x => x.CoinId.Equals(input.CoinId) && x.CompanyId.Equals(input.CompanyId)
                        , c => c.MoenyAction.Clearing.FromClient
                        , c => c.MoenyAction.Clearing.ToClient
                        , c => c.MoenyAction.Clearing.ToCompany
                        , c => c.MoenyAction.Clearing.ToCompany
                        , c => c.MoenyAction.Clearing.FromCompany
                        , c => c.MoenyAction.PublicMoney.PublicExpense
                        , c => c.MoenyAction.PublicMoney.PublicIncome
                        ).OrderBy(x => x.MoenyAction.Date);

                    var companyCashFlowsBeforeFromDate = companyCashFlows.Where(x => x.Created < input.From);
                    if (companyCashFlowsBeforeFromDate.Any())
                    {
                        var lastCompanyCashFlowBeforeFromDate = companyCashFlowsBeforeFromDate.LastOrDefault();
                        lastBalance = lastCompanyCashFlowBeforeFromDate.Total;
                    }

                    companyCashFlowsDtos.Add(
                            new CompanyCashFlowOutputDto()
                            {
                                Balance = lastBalance,
                                Type = "رصيد سابق"
                            });


                    var dataCashFlows = new List<CompanyCashFlow>();

                    if (input.From != null && input.To != null)
                    {
                        dataCashFlows = companyCashFlows.Where(x => x.Created >= input.From && x.Created <= input.To).ToList();
                    }
                    else if (input.From == null && input.To != null)
                    {
                        dataCashFlows = companyCashFlows.Where(x => x.Created <= input.To).ToList();
                    }
                    else if (input.From != null && input.To == null)
                    {
                        dataCashFlows = companyCashFlows.Where(x => x.Created >= input.From).ToList();
                    }
                    else
                    {
                        dataCashFlows = companyCashFlows.ToList();
                    }

                    foreach (var companyCashFlow in dataCashFlows)
                    {
                        companyCashFlowsDtos.Add(
                            new CompanyCashFlowOutputDto()
                            {
                                Id = companyCashFlow.Id,
                                Balance = companyCashFlow.Total,
                                Amount = companyCashFlow.Amount,
                                Commission = companyCashFlow.Commission(),
                                SecondCommission = companyCashFlow.SecounCommission(),
                                ReceiverName = companyCashFlow.ReceiverName(Requester.Company, input.CompanyId),
                                SenderName = companyCashFlow.SenderName(Requester.Company,input.CompanyId),
                                CountryName = companyCashFlow.CountryName(),
                                Type = companyCashFlow.MoenyAction.GetTypeName(Requester.Company, companyCashFlow.CompanyId),
                                Name = _moneyActionAppService.GetActionName(companyCashFlow.MoenyAction),
                                Number = companyCashFlow.MoenyAction.GetActionId(),
                                Date = companyCashFlow.Created != null ? companyCashFlow.Created.Value.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")) : string.Empty,
                                Note = companyCashFlow.MoenyAction.GetNote(Requester.Company, companyCashFlow.CompanyId),
                                MoneyActionId= companyCashFlow.MoneyActionId,
                                Matched=companyCashFlow.Matched
                            });
                    }
                }
                
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companyCashFlowsDtos;
        }

        public CompanyCashFlowDto GetById(int id)
        {
            CompanyCashFlowDto companyCashFlowDto = null;
            try
            {
                var companyCashFlow = _unitOfWork.GenericRepository<CompanyCashFlow>().GetById(id);
                if (companyCashFlow != null)
                {
                    companyCashFlowDto = Mapper.Map<CompanyCashFlow, CompanyCashFlowDto>(companyCashFlow);

                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companyCashFlowDto;
        }

     
        public CompanyCashFlowUpdateDto GetForEdit(int id)
        {
            CompanyCashFlowUpdateDto companyCashFlowDto = null;
            try
            {
                var companyCashFlow = _unitOfWork.GenericRepository<CompanyCashFlow>().GetById(id);
                if (companyCashFlow != null)
                {
                    companyCashFlowDto = Mapper.Map<CompanyCashFlow, CompanyCashFlowUpdateDto>(companyCashFlow);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companyCashFlowDto;
        }

        public IList<BalanceStatementDto> GetForStatement(int coinId, DateTime? date)
        {
            var companyCashFlowsDtos = new List<BalanceStatementDto>();
            try
            {
                var companyCashFlows = _unitOfWork.GenericRepository<CompanyCashFlow>()
                        .FindBy(x => x.CoinId.Equals(coinId));

                if (date != null)
                {
                    companyCashFlows = companyCashFlows.Where(x => x.Created <= date);
                }

                companyCashFlowsDtos = (from c in companyCashFlows
                                        select new BalanceStatementDto()
                                       {
                                           Name = c.Company != null ? c.Company.Name : string.Empty,
                                           Total = c.Total,
                                           Type = "الشركة "
                                       }).ToList();

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return companyCashFlowsDtos;
        }

        public CompanyCashFlowDto Insert(CompanyCashFlowInsertDto dto)
        {
            CompanyCashFlowDto companyCashFlowDto = null;
            try
            {
                var companyCashFlow = Mapper.Map<CompanyCashFlowInsertDto, CompanyCashFlow>(dto);
                companyCashFlow.CreatedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<CompanyCashFlow>().Insert(companyCashFlow);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                companyCashFlowDto = Mapper.Map<CompanyCashFlow, CompanyCashFlowDto>(companyCashFlow);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return companyCashFlowDto;
        }

        public CompanyCashFlowDto Update(CompanyCashFlowUpdateDto dto)
        {
            CompanyCashFlowDto companyCashFlowDto = null;
            try
            {
                var companyCashFlow = _unitOfWork.GenericRepository<CompanyCashFlow>().GetById(dto.Id);
                Mapper.Map<CompanyCashFlowUpdateDto, CompanyCashFlow>(dto, companyCashFlow);
                companyCashFlow.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<CompanyCashFlow>().Update(companyCashFlow);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                companyCashFlowDto = Mapper.Map<CompanyCashFlow, CompanyCashFlowDto>(companyCashFlow);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return companyCashFlowDto;
        }

        public void Delete(int id)
        {
            try
            {
                var companyCashFlow = _unitOfWork.GenericRepository<CompanyCashFlow>().GetById(id);
                if (companyCashFlow != null)
                {
                    _unitOfWork.GenericRepository<CompanyCashFlow>().Delete(companyCashFlow);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        public CompanyMatchDto ConvertMatchingStatus(CompanyMatchDto dto)
        {
            CompanyMatchDto output = null;
            try
            {
                _unitOfWork.CreateTransaction();

                var companyCashFlow = _unitOfWork.GenericRepository<CompanyCashFlow>().FindBy(x => x.Id == dto.CompanyCashFlowId).FirstOrDefault();
                if (companyCashFlow != null)
                {
                    companyCashFlow.Matched = dto.Matched;
                    var userName = _appSession.GetUserName();
                    if (!string.IsNullOrEmpty(userName))
                    {
                        
                        var currentUser = _unitOfWork.GenericRepository<User>().FindBy(x => x.UserName == userName).FirstOrDefault();
                        if (currentUser != null)
                        {
                            companyCashFlow.UserMatched = currentUser.UserId;
                        }
                    }
                    
                }

                _unitOfWork.Save();
                _unitOfWork.Commit();

                output = dto;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
            }

            return output;
        }
    }
}
