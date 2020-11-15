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
using System.Globalization;

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
            //decimal clientAmount = 0;
            //decimal companyAmount = 0;
            //decimal boxBalance = 0;
            //if (to.Date == DateTime.Now.Date && to.Month == DateTime.Now.Month && to.Year == DateTime.Now.Year)
            //{
            //    clientAmount = _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.CoinId == coinId).Sum(c => c.Total);
            //    companyAmount = _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CoinId == coinId).Sum(c => c.Total);
            //    boxBalance = _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == coinId).First().Total;
            //}
            //else
            //{
            //    clientAmount = _unitOfWork.GenericRepository<ClientCashFlow>().FindBy(c => c.CoinId == coinId & c.Created <= to)
            //   .GroupBy(c => c.ClientId)
            //   .Select(c => c.OrderByDescending(s => s.Id).FirstOrDefault())
            //   .Select(c => c.Total).DefaultIfEmpty(0).Sum();
            //    companyAmount = _unitOfWork.GenericRepository<CompanyCashFlow>().FindBy(c => c.CoinId == coinId)
            //    .Where(c => c.Created <= to)
            //    .GroupBy(c => c.CompanyId)
            //    .Select(c => c.OrderByDescending(s => s.Id).FirstOrDefault())
            //    .Select(c => c.Total).DefaultIfEmpty(0).Sum();
            //    boxBalance = _unitOfWork.GenericRepository<BranchCashFlow>().FindBy(c => c.CoinId == coinId && c.Created <= to)
            //   .LastOrDefault() != null ? _unitOfWork.GenericRepository<BranchCashFlow>().FindBy(c => c.CoinId == coinId && c.Created <= to)
            //   .LastOrDefault().Total : 0;
            //}
            //var transactionDontDeleivred = _unitOfWork.GenericRepository<Transaction>()
            //    .FindBy(c => c.CoinId == coinId && c.DeliverdDate <= to)
            //    .Select(c => c.Amount).DefaultIfEmpty(0).Sum();
            //var actualyBalnce = boxBalance - clientAmount - companyAmount - transactionDontDeleivred;
            decimal clientAmount = 0;
            decimal companyAmount = 0;
            decimal boxBalance = 0;
            clientAmount = _unitOfWork.GenericRepository<ClientCashFlow>().FindBy(c => c.CoinId == coinId).Sum(c => c.Amount);
            clientAmount += _unitOfWork.GenericRepository<ClientCash>().FindBy(c => c.CoinId == coinId).Sum(c => c.InitialBalance);
            companyAmount += _unitOfWork.GenericRepository<CompanyCashFlow>().FindBy(c => c.CoinId == coinId).Sum(c => c.Amount);
            companyAmount += _unitOfWork.GenericRepository<CompanyCash>().FindBy(c => c.CoinId == coinId).Sum(c => c.InitialBalance);
            boxBalance += _unitOfWork.GenericRepository<BranchCashFlow>().FindBy(c => c.CoinId == coinId).Sum(c => c.Amount);
            boxBalance += _unitOfWork.GenericRepository<BranchCash>().FindBy(c => c.CoinId == coinId).Sum(c => c.InitialBalance);
            var transactionDontDeleivred = _unitOfWork.GenericRepository<Transaction>()
                .FindBy(c => c.CoinId == coinId && c.DeliverdDate <= to)
                .Select(c => c.Amount).DefaultIfEmpty(0).Sum();
            var actualyBalnce = boxBalance - clientAmount + companyAmount + transactionDontDeleivred;
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

        public DataTablesDto GetPayment(int draw, int start, int length, int coinId, PaymentsTypeEnum paymentsTypeEnum, DateTime? from, DateTime? to, int? PaymentsEntitiyId)
        {
            //total count
            Expression<Func<MoneyAction, bool>> expression = c => (c.BoxAction != null && c.BoxAction.CoinId == coinId &&
            (c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasury || c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToClient || c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToCompany))
            || (c.Transaction != null && c.Transaction.Deliverd == true && c.Transaction.CoinId == coinId);
            var totalCount = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression).Count();
            IEnumerable<MoneyAction> moneyActions;
            if (paymentsTypeEnum == PaymentsTypeEnum.Non)
            {
                
                moneyActions = GetAllPayemnt(coinId, from, to);
            }
            else if (paymentsTypeEnum == PaymentsTypeEnum.Company)
            {
                moneyActions = GetCompanyPaymet(coinId, from, to, PaymentsEntitiyId);
            }
            else if (paymentsTypeEnum == PaymentsTypeEnum.Agent)
            {
                moneyActions = GetAgentPayment(coinId, from, to, PaymentsEntitiyId);
            }
            else if (paymentsTypeEnum == PaymentsTypeEnum.DirectTransaction)
            {
                moneyActions = this.GetDilverdTransaction(coinId, from, to, PaymentsEntitiyId);
            }
            else
            {
                moneyActions = GetPublicPayment(coinId, from, to, PaymentsEntitiyId);
            }

            var filterCount = moneyActions.Count();
            moneyActions = moneyActions.Skip(start).Take(length);
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            foreach (var item in moneyActions)
            {
                IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
                {
                    Date = item.Date.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")),
                    MoneyActionId = item.Id,
                    Note = item.GetNote(Requester.Branch, null)!=null? item.GetNote(Requester.Branch, null):"",
                    Amount = item.Transaction != null ? item.Transaction.Amount : item.BoxAction.Amount,
                    Type = item.Transaction != null ? "تسليم حولة" : item.BoxAction.GetActionType(),
                    Name = _moneyActionAppService.GetActionName(item)
                };
                incomeOutcomeReports.Add(incomeOutcomeReport);
            }
            DataTablesDto dt = new DataTablesDto(draw, incomeOutcomeReports, filterCount, totalCount);
            return dt;
        }
        private IEnumerable<MoneyAction> GetPublicPayment(int coinId, DateTime? from, DateTime? to, int? publicExId)
        {

            Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
                && c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasury;
            var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression,"BoxAction","PublicMoney.PublicExpense");
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
            return moneyAction;


        }
        private IEnumerable<MoneyAction> GetAllPayemnt(int coinId, DateTime? from, DateTime? to)
        {
            Expression<Func<MoneyAction, bool>> expression = c => (c.BoxAction != null && c.BoxAction.CoinId == coinId &&
            (c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasury || c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToClient || c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToCompany))
            || (c.Transaction != null && c.Transaction.Deliverd == true && c.Transaction.CoinId == coinId&&c.Transaction.TypeOfPay==Domain.Model.Settings.TypeOfPay.Cash);
            var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, "Transaction.SenderCompany", "Transaction.ReciverClient", "Transaction.SenderClient", "Transaction.ReceiverCompany", "BoxAction", "CompanyCashFlows.Company", "ClientCashFlows.Client");
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
            //foreach (var item in moneyAction)
            //{
            //    var incomeOutComeReport = new IncomeOutcomeReport()
            //    {
            //        MoneyActionId = item.Id,
            //        Date = item.Date,
            //        Name = _moneyActionAppService.GetActionName(item)
            //    };
            //    if (item.Transaction != null)
            //    {
            //        incomeOutComeReport.Amount = item.Transaction.Amount;
            //        incomeOutComeReport.Note = item.Transaction.Note;
            //        incomeOutComeReport.Type = "حوالة مباشرة";
            //    }
            //    else
            //    {
            //        incomeOutComeReport.Amount = item.BoxAction.Amount;
            //        incomeOutComeReport.Note = item.BoxAction.Note;
            //        incomeOutComeReport.Type = item.BoxAction.GetActionType();
            //    }
            //    incomeOutcomeReports.Add(incomeOutComeReport);
            //}
            return moneyAction;

        }
        private IEnumerable<MoneyAction> GetAgentPayment(int coinId, DateTime? from, DateTime? to, int? agentId)
        {
            //List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            //try
            //{
            Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId &&
             c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToClient;
            var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression,"BoxAction", "ClientCashFlows.Client");
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
            //foreach (var item in moneyAction)
            //{
            //    var incomeOutComeReport = new IncomeOutcomeReport()
            //    {
            //        MoneyActionId = item.Id,
            //        Date = item.Date,
            //        Amount = item.BoxAction.Amount,
            //        Note = item.BoxAction.Note,
            //        Type = item.BoxAction.GetActionType(),
            //        Name = _moneyActionAppService.GetActionName(item)
            //    };
            //    incomeOutcomeReports.Add(incomeOutComeReport);
            //}
            return moneyAction;
        }
        private IEnumerable<MoneyAction> GetCompanyPaymet(int coinId, DateTime? from, DateTime? to, int? companyId)
        {
            //List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            //try
            //{
            Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
             && c.BoxAction.BoxActionType == BoxActionType.ExpenseFromTreasuryToCompany;
            var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression,"BoxAction","CompanyCashFlows.Company");
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
            //foreach (var item in moneyAction)
            //{
            //    var incomeOutComeReport = new IncomeOutcomeReport()
            //    {
            //        MoneyActionId = item.Id,
            //        Date = item.Date,
            //        Amount = item.BoxAction.Amount,
            //        Note = item.BoxAction.Note,
            //        Type = item.BoxAction.GetActionType(),
            //        Name = _moneyActionAppService.GetActionName(item)
            //    };
            //    incomeOutcomeReports.Add(incomeOutComeReport);
            //}
            //}
            //catch (Exception ex)
            //{
            //    Tracing.SaveException(ex);
            //}
            //return incomeOutcomeReports;
            return moneyAction;
        }
        private IEnumerable<MoneyAction> GetDilverdTransaction(int coinId, DateTime? from, DateTime? to, int? clientId)
        {
            //List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            //try
            //{
            Expression<Func<MoneyAction, bool>> expression = c => c.Transaction != null && c.Transaction.Deliverd == true &&
             c.Transaction.CoinId == coinId&&c.Transaction.TypeOfPay==Domain.Model.Settings.TypeOfPay.Cash;
            var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, "Transaction.SenderCompany", "Transaction.ReciverClient", "Transaction.SenderClient", "Transaction.ReceiverCompany", "BoxAction", "CompanyCashFlows.Company", "ClientCashFlows.Client");
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
            //    foreach (var item in moneyAction)
            //    {
            //        IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
            //        {
            //            Amount = item.Transaction.Amount,
            //            Date = item.Date,
            //            MoneyActionId = item.Id,
            //            Name = _moneyActionAppService.GetActionName(item),
            //            Note = item.Transaction.Note,
            //            Type = "تسليم حوالة",
            //        };
            //        incomeOutcomeReports.Add(incomeOutcomeReport);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Tracing.SaveException(ex);
            //}
            //return incomeOutcomeReports;
            return moneyAction;
        }
        public DataTablesDto GetIncme(int draw, int start, int length, int coinId, PaymentsTypeEnum paymentsTypeEnum, DateTime? from, DateTime? to, int? incomeFromEntitiyId)
        {

            IEnumerable<MoneyAction> moneyActions;
            Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
                 && (c.BoxAction.BoxActionType == BoxActionType.ReceiveFromClientToTreasury
                 || c.BoxAction.BoxActionType == BoxActionType.ReceiveFromCompanyToTreasury
                 || c.BoxAction.BoxActionType == BoxActionType.ReceiveToTreasury);
            var total = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression, c => c.BoxAction).Count();


            //if (paymentsTypeEnum == PaymentsTypeEnum.DirectTransaction)
            //{
            //    //return new List<IncomeOutcomeReport>();
            //}
            if (paymentsTypeEnum == PaymentsTypeEnum.Non)
            {
                moneyActions = GetAllIncome(coinId, from, to);
            }
            else if (paymentsTypeEnum == PaymentsTypeEnum.Agent)
            {
                moneyActions = GetAgentInome(coinId, from, to, incomeFromEntitiyId);
            }
            else if (paymentsTypeEnum == PaymentsTypeEnum.Company)
            {
                moneyActions = GetCompanyIncome(coinId, from, to, incomeFromEntitiyId);
            }
            //if (paymentsTypeEnum == PaymentsTypeEnum.Public)
            else
            {
                moneyActions = GetPublicIncome(coinId, from, to, incomeFromEntitiyId);
            }
            var filterCount = moneyActions.Count();
            moneyActions = moneyActions.Skip(start).Take(length);
            List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            foreach (var item in moneyActions.ToList())
            {
                IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
                {
                    Date = item.Date.ToString("dd/MM/yyyy", new CultureInfo("ar-AE")),
                    MoneyActionId = item.Id,
                    Note = item.GetNote(Requester.Branch, null) != null ? item.GetNote(Requester.Branch, null) : "",
                    Amount = item.BoxAction.Amount,
                    Name = _moneyActionAppService.GetActionName(item),
                    Type = item.BoxAction.GetActionType()
                };
                incomeOutcomeReports.Add(incomeOutcomeReport);
            }
            DataTablesDto dt = new DataTablesDto(draw, incomeOutcomeReports, filterCount, total);
            return dt;
            //throw new Exception();
        }
        private IEnumerable<MoneyAction> GetAllIncome(int coinId, DateTime? from, DateTime? to)
        {
            //List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            //try
            //{
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
            //foreach (var item in moneyAction)
            //{
            //    IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
            //    {
            //        Amount = item.BoxAction.Amount,
            //        Date = item.Date,
            //        MoneyActionId = item.Id,
            //        Note = item.BoxAction.Note,
            //        Name = _moneyActionAppService.GetActionName(item),
            //        Type = item.BoxAction.GetActionType()
            //    };
            //    incomeOutcomeReports.Add(incomeOutcomeReport);
            //}
            //}
            //catch (Exception ex)
            //{
            //    Tracing.SaveException(ex);
            //}
            //return incomeOutcomeReports;
            return moneyAction;
        }
        private IEnumerable<MoneyAction> GetAgentInome(int coinId, DateTime? from, DateTime? to, int? agentId)
        {
            //List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            //try
            //{
            Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
             && (c.BoxAction.BoxActionType == BoxActionType.ReceiveFromClientToTreasury);
            var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression,"BoxAction", "ClientCashFlows.Client");
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
            //    foreach (var item in moneyAction)
            //    {
            //        IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
            //        {
            //            Amount = item.BoxAction.Amount,
            //            Date = item.Date,
            //            MoneyActionId = item.Id,
            //            Note = item.BoxAction.Note,
            //            Name = _moneyActionAppService.GetActionName(item),
            //            Type = item.BoxAction.GetActionType()
            //        };
            //        incomeOutcomeReports.Add(incomeOutcomeReport);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Tracing.SaveException(ex);
            //}
            //return incomeOutcomeReports;
            return moneyAction;
        }
        private IEnumerable<MoneyAction> GetCompanyIncome(int coinId, DateTime? from, DateTime? to, int? companyId)
        {
            //List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            //try
            //{
            Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
             && (c.BoxAction.BoxActionType == BoxActionType.ReceiveFromCompanyToTreasury);
            var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression,"BoxAction","CompanyCashFlows.Company");
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
            //    foreach (var item in moneyAction)
            //    {
            //        IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
            //        {
            //            Amount = item.BoxAction.Amount,
            //            Date = item.Date,
            //            MoneyActionId = item.Id,
            //            Note = item.BoxAction.Note,
            //            Name = _moneyActionAppService.GetActionName(item),
            //            Type = item.BoxAction.GetActionType()
            //        };
            //        incomeOutcomeReports.Add(incomeOutcomeReport);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Tracing.SaveException(ex);
            //}
            //return incomeOutcomeReports;
            return moneyAction;
        }
        private IEnumerable<MoneyAction> GetPublicIncome(int coinId, DateTime? from, DateTime? to, int? publicId)
        {
            //List<IncomeOutcomeReport> incomeOutcomeReports = new List<IncomeOutcomeReport>();
            //try
            //{
            Expression<Func<MoneyAction, bool>> expression = c => c.BoxAction != null && c.BoxAction.CoinId == coinId
             && (c.BoxAction.BoxActionType == BoxActionType.ReceiveToTreasury);
            var moneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression,"BoxAction","PublicMoney.PublicIncome");
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
            //    foreach (var item in moneyAction)
            //    {
            //        IncomeOutcomeReport incomeOutcomeReport = new IncomeOutcomeReport()
            //        {
            //            Amount = item.BoxAction.Amount,
            //            Date = item.Date,
            //            MoneyActionId = item.Id,
            //            Note = item.BoxAction.Note,
            //            Name = _moneyActionAppService.GetActionName(item),
            //            Type = item.BoxAction.GetActionType()
            //        };
            //        incomeOutcomeReports.Add(incomeOutcomeReport);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Tracing.SaveException(ex);
            //}
            //return incomeOutcomeReports;
            return moneyAction;
        }

        public IList<ClearigStatement> GetClearing(int coinId, IncomeOrOutCame incomeOrOutCame, DateTime? from, DateTime? to, ClearingAccountType fromAccountType, int? fromAccountId, ClearingAccountType toAccountType, int? toAccountId)
        {
            List<ClearigStatement> clearigStatements = new List<ClearigStatement>();
            try
            {
                #region expression Code
                //Expression<Func<MoneyAction, bool>> expression = c => c.Clearing != null && c.Clearing.CoinId == coinId;

                //var test = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression);
                ////expression.
                //if (incomeOrOutCame != IncomeOrOutCame.Non)
                //{


                //    Expression<Func<MoneyAction, bool>> incomeOrOutComeExperssion;
                //    if (incomeOrOutCame == IncomeOrOutCame.Income)
                //    {
                //        incomeOrOutComeExperssion = c => c.Clearing.IsIncome == true;
                //    }
                //    else
                //    {
                //        incomeOrOutComeExperssion = c => c.Clearing.IsIncome == false;
                //    }

                //    expression = Expression.Lambda<Func<MoneyAction, bool>>(Expression.AndAlso(expression.Body, incomeOrOutComeExperssion.Body), expression.Parameters[0]);
                //    var test2 = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression);
                //}
                //if (from != null)
                //{
                //    Expression<Func<MoneyAction, bool>> dateExp = c => c.Date >= from;
                //    expression = Expression.Lambda<Func<MoneyAction, bool>>(Expression.And(expression.Body, dateExp.Body), expression.Parameters[0]);
                //}
                //if (to != null)
                //{
                //    DateTime dto = ((DateTime)to).AddHours(24);
                //    Expression<Func<MoneyAction, bool>> dateExp = c => c.Date <= to;
                //    expression = Expression.Lambda<Func<MoneyAction, bool>>(Expression.And(expression.Body, dateExp.Body), expression.Parameters[0]);
                //}
                //if (fromAccountType != ClearingAccountType.All)
                //{
                //    Expression<Func<MoneyAction, bool>> fromAccountExpresion;
                //    if (fromAccountType == ClearingAccountType.Agent)
                //    {
                //        if (fromAccountId != null)
                //        {
                //            fromAccountExpresion = c => c.Clearing.FromClientId == fromAccountId;
                //        }
                //        else
                //        {
                //            fromAccountExpresion = c => c.Clearing.FromClientId != null;
                //        }
                //    }
                //    else
                //    {
                //        if (fromAccountId != null)
                //        {
                //            fromAccountExpresion = c => c.Clearing.FromCompanyId == fromAccountId;
                //        }
                //        else
                //        {
                //            fromAccountExpresion = c => c.Clearing.FromCompanyId != null;
                //        }
                //    }
                //    expression = Expression.Lambda<Func<MoneyAction, bool>>(Expression.And(expression.Body, fromAccountExpresion.Body), expression.Parameters[0]);
                //}
                //if (toAccountType != ClearingAccountType.All)
                //{
                //    Expression<Func<MoneyAction, bool>> toAccountExpresion;
                //    if (toAccountType == ClearingAccountType.Agent)
                //    {
                //        if (toAccountId != null)
                //        {
                //            toAccountExpresion = c => c.Clearing.ToClientId == toAccountId;
                //        }
                //        else
                //        {
                //            toAccountExpresion = c => c.Clearing.ToClientId != null;
                //        }
                //    }
                //    else
                //    {
                //        if (toAccountId != null)
                //        {
                //            toAccountExpresion = c => c.Clearing.ToCompanyId == toAccountId;
                //        }
                //        else
                //        {
                //            toAccountExpresion = c => c.Clearing.ToCompanyId != null;
                //        }
                //    }
                //    expression = Expression.Lambda<Func<MoneyAction, bool>>(Expression.And(expression.Body, toAccountExpresion.Body), expression.Parameters[0]);
                //}
                //var clearingMoneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(expression,c=>c.Clearing,c=>c.Clearing.FromClient, c => c.Clearing.ToClient, c => c.Clearing.FromCompany, c => c.Clearing.ToCompany);
                #endregion
                var clearingMoneyAction = _unitOfWork.GenericRepository<MoneyAction>().FindBy(c => c.Clearing != null && c.Clearing.CoinId == coinId, c => c.Clearing, c => c.Clearing.FromClient, c => c.Clearing.ToClient, c => c.Clearing.FromCompany, c => c.Clearing.ToCompany);
                if (incomeOrOutCame != IncomeOrOutCame.Non)
                {
                    if (incomeOrOutCame == IncomeOrOutCame.Income)
                    {
                        clearingMoneyAction = clearingMoneyAction.Where(c => c.Clearing.IsIncome == true);
                    }
                    else
                    {
                        clearingMoneyAction = clearingMoneyAction.Where(c => c.Clearing.IsIncome == false);
                    }
                    if (from != null)
                    {
                        clearingMoneyAction = clearingMoneyAction.Where(c => c.Date >= from);
                    }
                }
                if (to != null)
                {
                    DateTime dto = ((DateTime)to).AddHours(24);
                    clearingMoneyAction = clearingMoneyAction.Where(c => c.Date <= dto);
                }
                if (fromAccountType != ClearingAccountType.All)
                {
                    if (fromAccountType == ClearingAccountType.Agent)
                    {
                        if (fromAccountId != null)
                        {
                            clearingMoneyAction = clearingMoneyAction.Where(c => c.Clearing.FromClientId == fromAccountId);
                        }
                        else
                        {
                            clearingMoneyAction = clearingMoneyAction.Where(c => c.Clearing.FromClientId != null);
                        }
                    }
                    else
                    {
                        if (fromAccountId != null)
                        {
                            clearingMoneyAction = clearingMoneyAction.Where(c => c.Clearing.FromCompanyId == fromAccountId);
                        }
                        else
                        {
                            clearingMoneyAction = clearingMoneyAction.Where(c => c.Clearing.FromCompanyId != fromAccountId);
                        }
                    }
                }
                if (toAccountType != ClearingAccountType.All)
                {
                    if (toAccountType == ClearingAccountType.Agent)
                    {
                        if (toAccountId != null)
                        {
                            clearingMoneyAction = clearingMoneyAction.Where(c => c.Clearing.ToClientId == toAccountId);
                        }
                        else
                        {
                            clearingMoneyAction = clearingMoneyAction.Where(c => c.Clearing.ToClientId != toAccountId);
                        }
                    }
                    else
                    {
                        if (toAccountId != null)
                        {
                            clearingMoneyAction = clearingMoneyAction.Where(c => c.Clearing.ToCompanyId == toAccountId);
                        }
                        else
                        {
                            clearingMoneyAction = clearingMoneyAction.Where(c => c.Clearing.ToCompanyId != null);
                        }
                    }
                }
                foreach (var item in clearingMoneyAction)
                {
                    var cleraing = item.Clearing;
                    string tab;
                    if (cleraing.FromClient == null)
                    {
                        tab = "تبويب الشركات";
                    }
                    else
                    {
                        tab = "تبويب العملاء";
                    }
                    string fromName;
                    if (cleraing.FromClient != null)
                    {
                        fromName = cleraing.FromClient.FullName;
                    }
                    else
                    {
                        fromName = cleraing.FromCompany.Name;
                    }
                    string toName;
                    if (cleraing.ToClient != null)
                    {
                        toName = cleraing.ToClient.FullName;
                    }
                    else
                    {
                        toName = cleraing.ToCompany.Name;
                    }
                    ClearigStatement clearigStatement = new ClearigStatement()
                    {
                        Id = item.Id,
                        Amount = cleraing.Amount,
                        Note = cleraing.Note,
                        Date = item.Date,
                        Type = cleraing.IsIncome == true ? "قبض" : "صرف",
                        From = tab,
                        Name = fromName,
                        To = toName
                    };
                    clearigStatements.Add(clearigStatement);
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return clearigStatements;
        }
    }
}
