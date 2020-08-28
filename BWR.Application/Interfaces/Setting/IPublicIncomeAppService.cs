using BWR.Application.Dtos.Setting.PublicIncome;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Setting
{
    public interface IPublicIncomeAppService : IGrudAppService<PublicIncomeDto, PublicIncomeInsertDto, PublicIncomeUpdateDto>
    {
        IList<PublicIncomeForDropdownDto> GetForDropdown(string name);
       
        bool CheckIfExist(string name, int id);
    }
}
