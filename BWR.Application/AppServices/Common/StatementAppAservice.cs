using BWR.Application.Common;
using BWR.Application.Dtos.ReportsDto;
using BWR.Application.Dtos.Statement;
using BWR.Application.Interfaces;
using BWR.Application.Interfaces.Client;
using BWR.Application.Interfaces.Company;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Common;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Transactions;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using BWR.Domain.Model.Enums;
using BWR.Application.Extensions;
using BWR.Infrastructure.Exceptions;
using BWR.Application.Interfaces.Common;

namespace BWR.Application.AppServices.Common
{
    public class StatementAppService : IStatementAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;
        private readonly ICompanyCashFlowAppService _companyCashFlowAppService;
        private readonly IClientCashFlowAppService _clientCashFlowAppService;
        private readonly IMoneyActionAppService _moneyActionAppService;
        public StatementAppService(IUnitOfWork<MainContext> unitOfWork,
            ICompanyCashFlowAppService companyCashFlowAppService,
            IClientCashFlowAppService clientCashFlowAppService,
            IMoneyActionAppService moneyActionAppService,
            IAppSession appSession
            )
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
            _companyCashFlowAppService = companyCashFlowAppService;
            _clientCashFlowAppService = clientCashFlowAppService;
            _moneyActionAppService = moneyActionAppService;
        }

        public IList<BalanceStatementDto> GetAllBalances(int coinId, DateTime to)
        {
            var balances = _companyCashFlowAppService.GetForStatement(coinId, to).ToList();
            var clientBalances = _clientCashFlowAppService.GetForStatement(coinId, to).ToList();

            balances.AddRange(clientBalances);

            return balances;
        }


        public ConclusionDto GetConclusion(int coinId, DateTime to)
        {
            decimal clientAmount = 0;
            decimal companyAmount = 0;
            decimal boxBalance = 0;
            if (to.Date == DateTime.Now.Date && to.Month == DateTime.Now.Month && to.Year == DateTime.Now.Year)
            {
                clientAmount = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.CoinId == coinId).Sum(c => c.Total);
                companyAmount = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CoinId == coinId).Sum(c => c.Total);
                boxBalance = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == coinId).First().Total;
            }
            else
            {
                clientAmount = _unitOfWork.GenericRepository<ClientCashFlow>().FindBy(c => c.CoinId == coinId & c.Created <= to)
               .GroupBy(c => c.ClientId)
               .Select(c => c.OrderByDescending(s => s.Id).FirstOrDefault())
               .Select(c => c.Total).DefaultIfEmpty(0).Sum();
                companyAmount = _unitOfWork.GenericRepository<CompanyCashFlow>().FindBy(c => c.CoinId == coinId)
                .Where(c => c.Created <= to)
                .GroupBy(c => c.CompanyId)
                .Select(c => c.OrderByDescending(s => s.Id).FirstOrDefault())
                .Select(c => c.Total).DefaultIfEmpty(0).Sum();
                boxBalance = _unitOfWork.GenericRepository<BranchCashFlow>().FindBy(c => c.CoinId == coinId && c.Created <= to)
               .LastOrDefault() != null ? _unitOfWork.GenericRepository<BranchCashFlow>().FindBy(c => c.CoinId == coinId && c.Created <= to)
               .LastOrDefault().Total : 0;
            }
            var transactionDontDeleivred = _unitOfWork.GenericRepository<Transaction>()
                .FindBy(c => c.CoinId == coinId && c.DeliverdDate <= to)
                .Select(c => c.Amount).DefaultIfEmpty(0).Sum();
            var actualyBalnce = boxBalance - clientAmount - companyAmount - transactionDontDeleivred;

            var dto = new ConclusionDto
            {
                ClientAmount = clientAmount * -1,
                CompanyAmount = companyAmount * -1,
                NotDeliveredAmount = transactionDontDeleivred,
                TreasuryAmount = boxBalance,
                Total = actualyBalnce
            };

            return dto;
        }

        public IList<IncomeOutcomeReport> GetPayment(int coinId, PaymentsTypeEnum paymentsTypeEnum, DateTime? from, DateTime? to, int? PaymentsEntitiyId)
        {
            if (paymentsTypeEnum == PaymentsTypeEnum.Non)
            {
                return this.GetAllPayemnt(coinId, from, to);
            }
            if (paymentsTypeEnum == PaymentsTypeEnum.Company)
            {
                return GetCompanyPaymet(coinId, from, to, PaymentsEntitiyId);
            }
            if (paymentsTypeEnum == PaymentsTypeEnum.Agent)
            {
                return GetAgentPayment(coinId, from, to, PaymentsEntitiyId);
            }
            if (paymentsTypeEnum == PaymentsTypeEnum.DirectTransaction)
            {
                return this.GetDilverdTransaction(coinId, from, to, PaymentsEntitiyId);
            }
            if (paymentsTypeEnum == PaymentsTypeEnum.Public)
            {
                return GetPublicPayment(coinId, from, to, PaymentsEntitiyId);
            }
            throw new Exception("Payment Type Not Exist");
        }
        private IList<IncomeOutcomeReport> GetPublicPayment(int coinId, DateTime? from, DateTime? to, int? publicExId)
        {
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            try
            {
                Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
                && c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasury;
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, c => c.BoxAction);
                if (from != null)
                {
                    var dfrom = ((DateTime)from);
                    moneyAction = moneyAction.Where(c => c.Date >= dfrom);
                }
                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    moneyAction = moneyAction.Where(c => c.Date <= to);
                }
                if (publicExId != null)
                {
                    moneyAction = moneyAction.Where(c => c.PubLicMoneyId == publicExId);
                }
                foreach (var item in moneyAction)
                {
                    var incomeOutComeReport = new IncomeOutcomeReport()
                    {
                        MoneyActionId = item.Id,
                        Date = item.Date,
                        Amount = item.BoxAction.Amount,
                        Note = item.BoxAction.Note,
                        Type = item.BoxAction.GetActionType(),
                        Name = _moneyActionAppService.GetActionName(item)
                    };
                    incomeOutcomeReports.Add(incomeOutComeReport);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return incomeOutcomeReports;
        }
        private IList<IncomeOutcomeReport> GetAllPayemnt(int coinId, DateTime? from, DateTime? to)
        {
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            try
            {

                Expression<Func<MoneyAction, bool>> expression = c => (c.BoxAction != null && c.BoxAction.CoinId == coinId &&
                (c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasury || c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToClient || c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToCompany))
                || (c.Transaction != null && c.Transaction.Deliverd == true && c.Transaction.CoinId == coinId);
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, c => c.BoxAction, c => c.Transaction);
                if (from != null)
                {
                    var dfrom = ((DateTime)from);
                    moneyAction = moneyAction.Where(c => c.Date >= dfrom);
                }
                if (to != null)
                {
                    var dto = ((DateTime)to);
                    moneyAction = moneyAction.Where(c => c.Date <= to);
                }
                foreach (var item in moneyAction)
                {
                    var incomeOutComeReport = new IncomeOutcomeReport()
                    {
                        MoneyActionId = item.Id,
                        Date = item.Date,
                        Name = _moneyActionAppService.GetActionName(item)
                    };
                    if (item.Transaction != null)
                    {
                        incomeOutComeReport.Amount = item.Transaction.Amount;
                        incomeOutComeReport.Note = item.Transaction.Note;
                        incomeOutComeReport.Type = "حوالة مباشرة";
                    }
                    else
                    {
                        incomeOutComeReport.Amount = item.BoxAction.Amount;
                        incomeOutComeReport.Note = item.BoxAction.Note;
                        incomeOutComeReport.Type = item.BoxAction.GetActionType();
                    }
                    incomeOutcomeReports.Add(incomeOutComeReport);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return incomeOutcomeReports;
        }
        private IList<IncomeOutcomeReport> GetAgentPayment(int coinId, DateTime? from, DateTime? to, int? agentId)
        {
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            try
            {
                Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId &&
                 c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToClient;
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, c => c.BoxAction, c => c.ClientCashFlows);
                if (from != null)
                {
                    var dfrom = ((DateTime)from);
                    moneyAction = moneyAction.Where(c => c.Date >= dfrom);
                }
                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    moneyAction = moneyAction.Where(c => c.Date <= to);
                }
                if (agentId != null)
                {
                    moneyAction = moneyAction.Where(c => c.ClientCashFlows.First().Id == (int)agentId);
                }
                foreach (var item in moneyAction)
                {
                    var incomeOutComeReport = new IncomeOutcomeReport()
                    {
                        MoneyActionId = item.Id,
                        Date = item.Date,
                        Amount = item.BoxAction.Amount,
                        Note = item.BoxAction.Note,
                        Type = item.BoxAction.GetActionType(),
                        Name = _moneyActionAppService.GetActionName(item)
                    };
                    incomeOutcomeReports.Add(incomeOutComeReport);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return incomeOutcomeReports;
        }
        private IList<IncomeOutcomeReport> GetCompanyPaymet(int coinId, DateTime? from, DateTime? to, int? companyId)
        {
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            try
            {
                Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
                 && c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToCompany;
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, c => c.BoxAction, c => c.CompanyCashFlows);
                if (from != null)
                {
                    var dfrom = ((DateTime)from);
                    moneyAction = moneyAction.Where(c => c.Date >= dfrom);
                }
                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    moneyAction = moneyAction.Where(c => c.Date <= to);
                }
                if (companyId != null)
                {
                    moneyAction = moneyAction.Where(c => c.CompanyCashFlows.First().Id == (int)companyId);
                }
                foreach (var item in moneyAction)
                {
                    var incomeOutComeReport = new IncomeOutcomeReport()
                    {
                        MoneyActionId = item.Id,
                        Date = item.Date,
                        Amount = item.BoxAction.Amount,
                        Note = item.BoxAction.Note,
                        Type = item.BoxAction.GetActionType(),
                        Name = _moneyActionAppService.GetActionName(item)
                    };
                    incomeOutcomeReports.Add(incomeOutComeReport);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return incomeOutcomeReports;
        }
        private IList<IncomeOutcomeReport> GetDilverdTransaction(int coinId, DateTime? from, DateTime? to, int? clientId)
        {
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            try
            {
                Expression<Func<MoneyAction, bool>> expression = c => c.Transaction != null && c.Transaction.Deliverd == true &&
                 c.Transaction.CoinId == coinId;
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, c => c.Transaction);
                if (from != null)
                {
                    var dfrom = ((DateTime)from);
                    moneyAction = moneyAction.Where(c => c.Date >= dfrom);
                }
                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    moneyAction = moneyAction.Where(c => c.Date <= to);
                }
                if (clientId != null)
                {
                    moneyAction = moneyAction.Where(c => c.Transaction.ReciverClientId == (int)clientId);
                }
                foreach (var item in moneyAction)
                {
                    IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
                    {
                        Amount = item.Transaction.Amount,
                        Date = item.Date,
                        MoneyActionId = item.Id,
                        Name = _moneyActionAppService.GetActionName(item),
                        Note = item.Transaction.Note,
                        Type = "تسليم حوالة",
                    };
                    incomeOutcomeReports.Add(incomeOutcomeReport);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return incomeOutcomeReports;
        }
        public IList<IncomeOutcomeReport> GetIncme(int coinId, PaymentsTypeEnum paymentsTypeEnum, DateTime? from, DateTime? to, int? incomeFromEntitiyId)
        {
            if (paymentsTypeEnum == PaymentsTypeEnum.DirectTransaction)
            {
                return new List<IncomeOutcomeReport>();
            }
            if (paymentsTypeEnum == PaymentsTypeEnum.Non)
            {
                return GetAllIncome(coinId, from, to);
            }
            if (paymentsTypeEnum == PaymentsTypeEnum.Agent)
            {
               return this.GetAgentInome(coinId, from, to, incomeFromEntitiyId);
            }
            if (paymentsTypeEnum == PaymentsTypeEnum.Company)
            {
                return this.GetCompanyIncome(coinId, from, to, incomeFromEntitiyId);
            }
            if (paymentsTypeEnum == PaymentsTypeEnum.Public)
            {
                return this.GetPublicIncome(coinId, from, to, incomeFromEntitiyId);
            }
            throw new Exception("Income Type Not Exist");
        }
        private IList<IncomeOutcomeReport> GetAllIncome(int coinId, DateTime? from, DateTime? to)
        {
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            try
            {
                Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
                 && (c.BoxAction.BoxActionType == BoxActionType.ReceiveFromClientToTreasury
                 || c.BoxAction.BoxActionType == BoxActionType.ReceiveFromCompanyToTreasury
                 || c.BoxAction.BoxActionType == BoxActionType.ReceiveToTreasury);
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, c => c.BoxAction);
                if (from != null)
                {
                    var dfrom = ((DateTime)from);
                    moneyAction = moneyAction.Where(c => c.Date >= from);
                }
                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    moneyAction = moneyAction.Where(c => c.Date <= to);
                }
                foreach (var item in moneyAction)
                {
                    IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
                    {
                        Amount = item.BoxAction.Amount,
                        Date = item.Date,
                        MoneyActionId = item.Id,
                        Note = item.BoxAction.Note,
                        Name = _moneyActionAppService.GetActionName(item),
                        Type = item.BoxAction.GetActionType()
                    };
                    incomeOutcomeReports.Add(incomeOutcomeReport);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return incomeOutcomeReports;
        }
        private IList<IncomeOutcomeReport> GetAgentInome(int coinId, DateTime? from, DateTime? to, int? agentId)
        {
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            try
            {
                Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
                 && (c.BoxAction.BoxActionType == BoxActionType.ReceiveFromClientToTreasury);
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, c => c.BoxAction, c => c.ClientCashFlows);
                if (from != null)
                {
                    var dfrom = ((DateTime)from);
                    moneyAction = moneyAction.Where(c => c.Date >= from);
                }
                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    moneyAction = moneyAction.Where(c => c.Date <= to);
                }
                if (agentId != null)
                {
                    moneyAction = moneyAction.Where(c => c.ClientCashFlows.Any(a => a.Id == (int)agentId));
                }
                foreach (var item in moneyAction)
                {
                    IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
                    {
                        Amount = item.BoxAction.Amount,
                        Date = item.Date,
                        MoneyActionId = item.Id,
                        Note = item.BoxAction.Note,
                        Name = _moneyActionAppService.GetActionName(item),
                        Type = item.BoxAction.GetActionType()
                    };
                    incomeOutcomeReports.Add(incomeOutcomeReport);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return incomeOutcomeReports;
        }
        private IList<IncomeOutcomeReport> GetCompanyIncome(int coinId, DateTime? from, DateTime? to, int? companyId)
        {
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            try
            {
                Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
                 && (c.BoxAction.BoxActionType == BoxActionType.ReceiveFromCompanyToTreasury);
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, c => c.BoxAction, c => c.CompanyCashFlows);
                if (from != null)
                {
                    var dfrom = ((DateTime)from);
                    moneyAction = moneyAction.Where(c => c.Date >= from);
                }
                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    moneyAction = moneyAction.Where(c => c.Date <= to);
                }
                if (companyId != null)
                {
                    moneyAction = moneyAction.Where(c => c.CompanyCashFlows.Any(a => a.Id == (int)companyId));
                }
                foreach (var item in moneyAction)
                {
                    IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
                    {
                        Amount = item.BoxAction.Amount,
                        Date = item.Date,
                        MoneyActionId = item.Id,
                        Note = item.BoxAction.Note,
                        Name = _moneyActionAppService.GetActionName(item),
                        Type = item.BoxAction.GetActionType()
                    };
                    incomeOutcomeReports.Add(incomeOutcomeReport);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return incomeOutcomeReports;
        }
        private IList<IncomeOutcomeReport> GetPublicIncome(int coinId, DateTime? from, DateTime? to, int? publicId)
        {
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            try
            {
                Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
                 && (c.BoxAction.BoxActionType == BoxActionType.ReceiveToTreasury);
                var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, c => c.BoxAction);
                if (from != null)
                {
                    var dfrom = ((DateTime)from);
                    moneyAction = moneyAction.Where(c => c.Date >= from);
                }
                if (to != null)
                {
                    var dto = ((DateTime)to).AddHours(24);
                    moneyAction = moneyAction.Where(c => c.Date <= to);
                }
                if (publicId != null)
                {
                    moneyAction = moneyAction.Where(c => c.PubLicMoneyId == publicId);
                }
                foreach (var item in moneyAction)
                {
                    IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
                    {
                        Amount = item.BoxAction.Amount,
                        Date = item.Date,
                        MoneyActionId = item.Id,
                        Note = item.BoxAction.Note,
                        Name = _moneyActionAppService.GetActionName(item),
                        Type = item.BoxAction.GetActionType()
                    };
                    incomeOutcomeReports.Add(incomeOutcomeReport);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return incomeOutcomeReports;
        }
    }
}
