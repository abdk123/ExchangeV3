using BWR.Application.Common;
using BWR.Application.Dtos.Statement;
using BWR.Application.Dtos.Transaction.InnerTransaction;
using BWR.Domain.Model.Settings;
using BWR.Domain.Model.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BWR.Application.Interfaces.Transaction
{
    public interface IInnerTransactionAppService
    {
        IList<InnerTransactionDto> GetTransactions();
        InnerTransactionDto GetById(int transactionId);
        InnerTransactionUpdateDto GetForEdit(int transactionId);
        InnerTransactionInsertInitialDto InitialInputData();
        bool SaveInnerTransactions(InnerTransactionInsertListDto incometransacrions);
        bool EditInnerTransaction(InnerTransactionUpdateDto dto);
        DataTablesDto InnerTransactionStatementDetailed(int draw, int start, int length, int? reciverCompanyId, TypeOfPay typeOfPay, int? reciverId, int? senderCompanyId, int? senderClientId, int? coinId, TransactionStatus transactionStatus, DateTime? from, DateTime? to, bool? isDelivered);
        DataTablesDto IncmoeTransactionGross(int draw, int start, int length, int? companyId, DateTime? from, DateTime? to);
        DataTablesDto TransactionDontDileverd(int draw, int start, int length, TransactionStatus transactionStatus,int? clientId, int? companyId,int? coinId, DateTime? from, DateTime? to);


    }
}
