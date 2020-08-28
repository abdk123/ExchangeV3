using BWR.Application.Common;

namespace BWR.Application.Dtos.Common
{
    public class PhoneBaseDto : EntityDto
    {
        public string Phone { get; set; }
        public bool IsEnabled { get; set; }
    }
}
