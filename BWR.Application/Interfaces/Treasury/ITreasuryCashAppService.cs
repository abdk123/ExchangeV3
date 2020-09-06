using System.Collections.Generic;
using System.Threading.Tasks;
using BWR.Application.Dtos.Treasury;

namespace BWR.Application.Interfaces.Treasury
{
    public interface ITreasuryCashAppService
    {
        IList<TreasuryCashDto> GetTreasuryCashes(int treasuryId);
        TreasuryCashDto Insert(TreasuryCashDto dto);
        TreasuryCashDto Update(TreasuryCashDto dto);
        Task<TreasuryCashDto> InsertAsync(TreasuryCashDto treasuryInsertDto);
    }
}
