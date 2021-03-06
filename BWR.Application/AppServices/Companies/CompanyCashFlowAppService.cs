﻿using AutoMapper;
using BWR.Application.Dtos.Company.CompanyCashFlow;
using BWR.Application.Extensions;
using BWR.Application.Interfaces.Common;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Enums;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BWR.Application.Interfaces.Company;
using System;
using BWR.Application.Dtos.Statement;
using BWR.Domain.Model.Security;
using BWR.Domain.Model.Branches;

namespace BWR.Application.AppServices.Companies
{
    public class CompanyCashFlowAppService : ICompanyCashFlowAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        //private readonly IMoneyActionAppService _moneyActionAppService;
        private readonly IAppSession _appSession;

        public CompanyCashFlowAppService(
            IUnitOfWork<MainContext> unitOfWork,
            IMoneyActionAppService moneyActionAppService,
            IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            //_moneyActionAppService = moneyActionAppService;
            _appSession = appSession;
        }

        public IList<CompanyCashFlowOutputDto> Get(CompanyCashFlowInputDto input)
        {
            var companyCashFlowsDtos = new List<CompanyCashFlowOutputDto>();
            try
            {
                if (input.CoinId != 0)
                {
                    decimal lastBalance = 0;
                    var companyCash = _unitOfWork.GenericRepository<CompanyCash>()
                        .FindBy(x => x.CoinId == input.CoinId && x.CompanyId == input.CompanyId).FirstOrDefault();
                    if (companyCash != null)
                    {
                        lastBalance = companyCash.InitialBalance;
                    }
                    var companyCashFlows = _unitOfWork.GenericRepository<CompanyCashFlow>()
                        .FindBy(
                        x => x.CoinId.Equals(input.CoinId) 
                        && x.CompanyId.Equals(input.CompanyId)
                    //    , c => c.MoenyAction, c => c.MoenyAction.BoxAction, c => c.MoenyAction.Clearing.ToClient, c => c.MoenyAction.Clearing.ToCompany, c => c.MoenyAction.Clearing.FromClient, c => c.MoenyAction.Clearing.FromCompany
                    //, c => c.MoenyAction.Exchange.FirstCoin, c => c.MoenyAction.Exchange.SecoundCoin, c => c.MoenyAction.Exchange.MainCoin, c => c.MoenyAction.PublicMoney.PublicExpense, c => c.MoenyAction.PublicMoney.PublicIncome
                    //, c => c.MoenyAction.Transaction.ReciverClient, c => c.MoenyAction.Transaction.ReceiverCompany, c => c.MoenyAction.Transaction.SenderCompany, c => c.MoenyAction.Transaction.SenderClient, c => c.MoenyAction.Transaction.Coin
                    //, c => c.MoenyAction.ClientCashFlows, c => c.MoenyAction.CompanyCashFlows
                        );
                    if (input.From != null)
                    {
                        companyCashFlows = companyCashFlows.Where(c => c.MoenyAction.Date >= input.From);
                        lastBalance = companyCashFlows.Where(c => c.MoenyAction.Date < input.From).Sum(c => c.Amount);
                    }
                    if (input.To != null)
                    {
                        companyCashFlows = companyCashFlows.Where(c => c.MoenyAction.Date <= input.To);
                    }
                    companyCashFlowsDtos.Add(
                            new CompanyCashFlowOutputDto()
                            {
                                Balance = lastBalance,
                                Type = "رصيد سابق"
                            });
                    var list = companyCashFlows.ToList();
                    var dataCashFlows = companyCashFlows.OrderBy(c => c.MoenyAction.Date).ThenBy(c => c.MoneyActionId).ToList();
                    foreach (var companyCashFlow in dataCashFlows)
                    {
                        var temp = new CompanyCashFlowOutputDto()
                        {
                            Id = companyCashFlow.Id,
                            //Balance = companyCashFlowsDtos.Last().Balance + companyCashFlow.Amount,
                            Amount = companyCashFlow.Amount,
                            Commission = companyCashFlow.Commission(),
                            SecondCommission = companyCashFlow.SecounCommission(),
                            ReceiverName = companyCashFlow.ReceiverName(Requester.Company, input.CompanyId),
                            SenderName = companyCashFlow.SenderName(Requester.Company, input.CompanyId),
                            CountryName = companyCashFlow.CountryName(),
                            Type = companyCashFlow.MoenyAction.GetTypeName(Requester.Company, companyCashFlow.CompanyId),
                            Name = companyCashFlow.MoenyAction.GetActionName(),
                            Number = companyCashFlow.MoenyAction.GetActionId(),
                            Date = companyCashFlow.MoenyAction.Date != null ? companyCashFlow.MoenyAction.Date.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")) : string.Empty,
                            Note = companyCashFlow.MoenyAction.GetNote(Requester.Company, companyCashFlow.CompanyId),
                            MoneyActionId = companyCashFlow.MoneyActionId,
                            Matched = companyCashFlow.Matched,
                            Shaded = companyCashFlow.Shaded == true,
                            UserMatched = companyCashFlow.User?.FullName ?? "",
                            CompanyUserMatched = companyCashFlow.CompanyUser?.Name ?? ""
                        };
                        temp.Balance = companyCashFlowsDtos.Last().Balance + companyCashFlow.RealAmount;
                        //temp.Balance += temp.Commission;
                        //temp.Balance += temp.SecondCommission;
                        companyCashFlowsDtos.Add(temp);

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

        public IList<BalanceStatementDto> GetForStatement(int coinId, DateTime date)
        {
            var companyCashFlowsDtos = new List<BalanceStatementDto>();
            date = date.AddHours(24);
            try
            {

                var compaies = _unitOfWork.GenericRepository<Company>().GetAll();

                foreach (var item in compaies)
                {
                    decimal total = 0;
                    var initalBalance = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CoinId == coinId && c.CompanyId == item.Id).First().InitialBalance;
                    var cashFlow = _unitOfWork.GenericRepository<CompanyCashFlow>().FindBy(c => c.CoinId == coinId && c.CompanyId == item.Id && c.MoenyAction.Date <= date);
                    if (cashFlow.Any())
                    {
                        total += cashFlow.Sum(c => c.Amount);
                    }
                    total += initalBalance;
                    companyCashFlowsDtos.Add(new BalanceStatementDto()
                    {
                        Name = item.Name,
                        Type = "شركة",
                        Total = total
                    });
                }

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

        public IList<CompanyBalanceDto> GetBalanceForCompany(int companyId, int coinId)
        {
            var companyBalances = new List<CompanyBalanceDto>();
            var mainCoin = _unitOfWork.GenericRepository<BranchCash>().FindBy(x => x.IsMainCoin).FirstOrDefault().Coin;
            if (mainCoin != null)
            {
                var companyCashFlows = _unitOfWork.GenericRepository<CompanyCashFlow>().GetIQueryable()
               .Where(x => x.CompanyId == companyId);

                var mainCompanyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(x => x.CoinId == mainCoin.Id).FirstOrDefault();
                companyBalances.Add(new CompanyBalanceDto()
                {
                    IsMainCoin = true,
                    Balance = companyCashFlows.Where(x => x.CoinId == mainCoin.Id).Sum(x => x.Amount) + mainCompanyCash.InitialBalance,
                    CoinId = mainCoin.Id
                });

                var companyCash = _unitOfWork.GenericRepository<CompanyCash>().FindBy(x => x.CoinId == coinId).FirstOrDefault();
                companyBalances.Add(new CompanyBalanceDto()
                {
                    IsMainCoin = false,
                    Balance = companyCashFlows.Where(x => x.CoinId == coinId).Sum(x => x.Amount) + companyCash.InitialBalance,
                    CoinId = coinId
                });
            }

            return companyBalances;
        }
        public decimal GetCompanyBalanceBeforeAction(int coinId, int companyId , int moneyActionId)
        {
            return 0;
        }
        public void Shaded(int id, bool value)
        {
            try
            {
                var companyCahsFlow = this._unitOfWork.GenericRepository<CompanyCashFlow>().GetById(id);
                companyCahsFlow.Shaded = value;
                _unitOfWork.GenericRepository<CompanyCashFlow>().Update(companyCahsFlow);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                throw ex;
            }
        }
    }
}
