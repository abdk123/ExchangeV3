using BWR.Application.Dtos.Transaction.OuterTransaction;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Transaction
{
    public interface IOuterTransactionAppService
    {
        IList<OuterTransactionDto> GetTransactions(OuterTransactionInputDto input);
        OuterTransactionDto GetTransactionById(int id);
        OuterTransactionInsertInitialDto InitialInputData();
        bool OuterClientTransaction(OuterTransactionInsertDto input);
        bool OuterAgentTransaction(OuterTransactionInsertDto input);
        bool OuterCompanyTranasction(OuterTransactionInsertDto input);
    }
}
