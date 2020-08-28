using BWR.Application.Dtos.Setting.PublicExpense;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Setting
{
    public interface IPublicExpenseAppService : IGrudAppService<PublicExpenseDto, PublicExpenseInsertDto, PublicExpenseUpdateDto>
    {
        IList<PublicExpenseForDropdownDto> GetForDropdown(string name);

        bool CheckIfExist(string name, int id);
    }
}
