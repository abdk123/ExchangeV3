using BWR.Application.Common;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Client;
using BWR.Application.Dtos.Company;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Dtos.Setting.Country;
using BWR.Application.Dtos.Treasury;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using System;

namespace BWR.Application.Dtos.Transaction.InnerTransaction
{
    public class InnerTransactionDto: EntityDto
    {
        public string Note { get; set; }
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public decimal OurComission { get; set; }
        public bool? Deliverd { get; set; }
        public decimal? RecivingAmount { get; set; }
        public DateTime? DeliverdDate { get; set; }
        public DateTime? Created { get; set; }

        #region Sender Info
        public int? SenderBranchId { get; set; }
        
        public virtual BranchDto SenderBranch { get; set; }

        public int? SenderClientId { get; set; }
        
        public virtual ClientDto SenderClient { get; set; }

        public decimal? SenderCleirntCommission { get; set; }

        public int? SenderCompanyId { get; set; }
        
        public virtual CompanyDto SenderCompany { get; set; }

        public decimal? SenderCompanyComission { get; set; }
        #endregion

        #region Receiver Info
        public int ReceiverBranchId { get; set; }
        
        public virtual BranchDto ReceiverBranch { get; set; }

        public int? ReciverClientId { get; set; }
        
        public virtual ClientDto ReciverClient { get; set; }

        public decimal? ReciverClientCommission { get; set; }

        public int? ReceiverCompanyId { get; set; }
        
        public virtual CompanyDto ReceiverCompany { get; set; }

        public decimal? ReceiverCompanyComission { get; set; }

        #endregion

        public int CoinId { get; set; }
        
        public virtual CoinDto Coin { get; set; }

        public int? CountryId { get; set; }
        
        public virtual CountryDto Country { get; set; }
        
        public virtual TransactionStatus TransactionsStatus { get; set; }

        public virtual TransactionType TransactionType { get; set; }

        public int TreaseryId { get; set; }
        
        public virtual TreasuryDto Treasury { get; set; }

        public virtual TypeOfPay TypeOfPay { get; set; }
    }
}
