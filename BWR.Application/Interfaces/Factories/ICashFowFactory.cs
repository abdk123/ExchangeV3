using BWR.Application.Dtos.Common;
using BWR.Domain.Model.Common;
using BWR.ShareKernel.Common;

namespace BWR.Application.Interfaces.Factories
{
    public interface ICashFowFactory
    {
        Entity Create(MoneyAction moneyAction, int coinId, int id, decimal total, decimal amount, string createdBy = "");
    }
}
