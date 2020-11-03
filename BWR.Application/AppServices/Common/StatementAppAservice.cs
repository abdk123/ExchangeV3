using BWR.Application.Dtos.Statement;
using BWR.Application.Interfaces;
using BWR.Application.Interfaces.Client;
using BWR.Application.Interfaces.Company;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Branches;
using BWR.Domain.Model.Clients;
using BWR.Domain.Model.Companies;
using BWR.Domain.Model.Transactions;
using BWR.Infrastructure.Context;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.AppServices.Common
{
    public class StatementAppService : IStatementAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;
        private readonly ICompanyCashFlowAppService _companyCashFlowAppService;
        private readonly IClientCashFlowAppService _clientCashFlowAppService;

        public StatementAppService(IUnitOfWork<MainContext> unitOfWork,
            ICompanyCashFlowAppService companyCashFlowAppService,
            IClientCashFlowAppService clientCashFlowAppService,
            IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
            _companyCashFlowAppService = companyCashFlowAppService;
            _clientCashFlowAppService = clientCashFlowAppService;
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
    }
}
