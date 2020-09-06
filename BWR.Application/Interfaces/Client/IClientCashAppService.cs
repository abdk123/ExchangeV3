using System.Collections.Generic;
using System.Threading.Tasks;
using BWR.Application.Dtos.Client;
namespace BWR.Application.Interfaces.Client
{
    public interface IClientCashAppService
    {
        IList<ClientCashDto> GetAll();
        ClientCashDto GetById(int id);
        IList<ClientCashesDto> GetClientCashes(int companyId);
        ClientCashDto Insert(ClientCashDto dto);
        ClientCashDto Update(ClientCashDto dto);
        ClientCashDto UpdateBalance(ClientCashesDto dto);
        void Delete(int id);
        Task<ClientCashDto> InsertAsync(ClientCashDto clientCashInsertDto);
    }
}
