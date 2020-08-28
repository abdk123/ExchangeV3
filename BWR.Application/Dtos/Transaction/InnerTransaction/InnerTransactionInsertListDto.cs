using System.Collections.Generic;

namespace BWR.Application.Dtos.Transaction.InnerTransaction
{
    public class InnerTransactionInsertListDto
    {
        public int MainCompanyId { get; set; }
        public string Note { get; set; }
        public List<InnerTransactionInsertDto> Transactions { get; set; }
    }
}
