using BWR.Application.Dtos.Client;
using BWR.Domain.Model.Clients;
using System.Linq;

namespace BWR.Application.Extensions
{
    public static class ClientExtension
    {
        public static string GetFirstPhone(this Client client)
        {
            if (!client.ClientPhones.Any())
                return string.Empty;
            return client.ClientPhones[0].Phone;
        }

        public static string GetFirstPhone(this ClientDto client)
        {
            if (client != null || client.ClientPhones.Any())
                return client.ClientPhones[0].Phone;
            return string.Empty;
        }
    }
}
