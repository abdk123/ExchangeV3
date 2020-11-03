using BWR.Application.Dtos.Setting.Coin;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Setting
{
    public interface ICoinAppService : IGrudAppService<CoinDto, CoinInsertDto, CoinUpdateDto>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">filter by name</param>
        /// <returns></returns>
        IList<CoinForDropdownDto> GetForDropdown(string name);
        /// <summary>
        /// get all for dropdown
        /// </summary>
        /// <returns></returns>
        IList<CoinForDropdownDto> GetForDropdown();
        
        bool CheckIfExist(string name, int id);
    }
}
