using BWR.Application.Common;
using BWR.Application.Dtos.Common;

namespace BWR.Application.Dtos.Client
{
    public class ClientPhoneDto : PhoneBaseDto
    {
        public int ClientId { get; set; }
    }
}