using BWR.Application.Common;
using BWR.Application.Dtos.Setting.Country;

namespace BWR.Application.Dtos.Setting.PublicIncome
{
    public class PublicIncomeDto : EntityDto
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }

    }
}
