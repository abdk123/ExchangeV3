using BWR.Application.Extensions;
using BWR.Application.Interfaces.Common;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Settings;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BWR.Application.Dtos.Common;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using System;
using BWR.Application.Dtos.MoneyAction;

namespace BWR.Application.AppServices.Common
{
    public class MoneyActionAppService: IMoneyActionAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public MoneyActionAppService(IUnitOfWork<MainContext> unitOfWork
            , IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public string GetActionName(MoneyAction moneyAction)
        {
            try
            {
                if (moneyAction.Transaction != null && moneyAction.BoxAction == null)
                    return moneyAction.Transaction.GetActionName();
                if (moneyAction.PublicMoney != null)
                    return moneyAction.PublicMoney.GetActionName();
                if (moneyAction.BoxAction != null)
                {
                    if (moneyAction.ClientCashFlows != null && moneyAction.ClientCashFlows.Count > 0)
                        return new List<ClientCashFlow>(moneyAction.ClientCashFlows)[0].Client.FullName;
                    return new List<CompanyCashFlow>(moneyAction.CompanyCashFlows)[0].CompanyName();
                }
                if (moneyAction.Exchange != null)
                {
                    return _unitOfWork.GenericRepository<Coin>().GetById(moneyAction.Exchange.SecoundCoinId).Name;
                }
            }
            catch(Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return "GetActionName";
        }

        public MoneyActionOutputDto GetById(int id)
        {
            MoneyActionOutputDto moneyActionDto = null;
            try
            {
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().GetById(id);
                if (moneyAction != null)
                {
                    moneyActionDto = Mapper.Map<MoneyAction, MoneyActionOutputDto>(moneyAction);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return moneyActionDto;
        }

        public IList<MoneyActionDetailDto> GetByTransactionId(int transactionId)
        {
            IList<MoneyActionDetailDto> moneyActionDetailsDto = new List<MoneyActionDetailDto>();
            try
            {
                var moneyActions = _unitOfWork.GenericRepository<MoneyAction>()
                    .FindBy(x => x.TransactionId == transactionId).ToList();
                //var moneyActions = _unitOfWork.GenericRepository<MoneyAction>()
                //    .FindBy(x => x.TransactionId == transactionId, c => c.CompanyCashFlows, c => c.ClientCashFlows, c => c.BranchCashFlows).ToList();

                moneyActionDetailsDto = Mapper.Map<List<MoneyAction>, List<MoneyActionDetailDto>>(moneyActions);
            }
            catch (Exception exception)
            {
                Tracing.SaveException(exception);
            }

            return moneyActionDetailsDto;
        }
    }
}
