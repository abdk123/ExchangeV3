using BWR.Application.Dtos.Setting.Coin;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Setting
{
    public interface ICoinAppService : IGrudAppService<CoinDto, CoinInsertDto, CoinUpdateDto>
    {
        IList<CoinForDropdownDto> GetForDropdown(string name);
        bool CheckIfExist(string name, int id);
    }
}
