using BWR.Application.Common;
using BWR.Application.Dtos.Setting.Country;

namespace BWR.Application.Dtos.Setting.PublicExpense
{
    public class PublicExpenseDto : EntityDto
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        

    }
}
