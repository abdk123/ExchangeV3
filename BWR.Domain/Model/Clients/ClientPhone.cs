using BWR.Domain.Model.Common;

namespace BWR.Domain.Model.Clients
{
    public class ClientPhone: PhoneBase
    {
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
    }
}