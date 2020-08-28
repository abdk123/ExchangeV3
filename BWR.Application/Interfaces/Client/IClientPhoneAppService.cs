using System.Collections.Generic;
using BWR.Application.Dtos.Client;
namespace BWR.Application.Interfaces.Client
{
    public interface IClientPhoneAppService
    {
        IList<ClientPhoneDto> GetAll();
        ClientPhoneDto GetById(int id);
        ClientPhoneDto Insert(ClientPhoneDto dto);
        ClientPhoneDto Update(ClientPhoneDto dto);
        void Delete(int id);
    }
}
