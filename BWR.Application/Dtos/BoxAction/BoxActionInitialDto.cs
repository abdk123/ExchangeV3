using BWR.Application.Dtos.Client;
using BWR.Application.Dtos.Company;
using BWR.Application.Dtos.Setting.Attachment;
using BWR.Application.Dtos.Setting.Coin;
using BWR.Application.Dtos.Setting.Country;
using BWR.Application.Dtos.Setting.PublicExpense;
using BWR.Application.Dtos.Setting.PublicIncome;
using BWR.Application.Dtos.Treasury;
using BWR.Domain.Model.Treasures;
using System.Collections.Generic;

namespace BWR.Application.Dtos.BoxAction
{
    public class BoxActionInitialDto
    {
        public BoxActionInitialDto()
        {
            Coins = new List<CoinForDropdownDto>();
            Companies = new List<CompanyForDropdownDto>();
            Agents = new List<ClientDto>();
            PublicExpenses = new List<PublicExpenseForDropdownDto>();
            PublicIncomes = new List<PublicIncomeForDropdownDto>();
        }
        public int TreasuryId { get; set; }
        public IList<CoinForDropdownDto> Coins { get; set; }
        public IList<CompanyForDropdownDto> Companies { get; set; }
        public IList<ClientDto> Agents { get; set; }
        public IList<PublicExpenseForDropdownDto> PublicExpenses { get; set; }
        public IList<PublicIncomeForDropdownDto> PublicIncomes { get; set; }

    }
}
