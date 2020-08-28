using BWR.Application.Common;

namespace BWR.Application.Dtos.Client
{
    public class ClientTypeDto : EntityDto
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }
}