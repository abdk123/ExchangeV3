using BWR.Application.Dtos.Treasury;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Treasury
{
    public interface ITreasuryAppService : IGrudAppService<TreasuryDto, TreasuryInsertDto, TreasuryUpdateDto>
    {
        bool CheckIfExist(string name, int id);
        IList<TreasurysDto> GetAllWithBalances();
        TreasuryDto GetTreasuryForUser(string userName);
    }
}
