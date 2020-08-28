using System.Collections.Generic;
using BWR.Application.Dtos.Treasury.TreasuryMoneyAction;

namespace BWR.Application.Interfaces.TreasuryMoneyAction
{
    public interface ITreasuryMoneyActionAppService
    {
        IList<TreasuryMoneyActionDto> Get(TreasuryMoneyActionInputDto input);
        IList<TreasuryActionsDto> GetMoneyActions(TreasuryMoneyActionInputDto input);
        TreasuryMoneyActionDto GetMony(TreasuryMoneyActionInsertDto input);
        TreasuryMoneyActionDto GiveMony(TreasuryMoneyActionInsertDto input);
    }
}
