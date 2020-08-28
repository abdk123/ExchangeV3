
using BWR.Application.Dtos.Client;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Client
{
    public interface IClientAttatchmentAppService
    {
        IList<ClientAttatchmentDto> GetAll();
        IList<ClientAttatchmentDto> GetForSpecificClient(int clientId);
        ClientAttatchmentDto GetById(int id);
        ClientAttatchmentDto Insert(ClientAttatchmentDto dto);
        ClientAttatchmentDto Update(ClientAttatchmentDto dto);
        void Delete(int id);
    }
}
