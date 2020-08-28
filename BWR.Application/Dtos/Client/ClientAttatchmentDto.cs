using BWR.Application.Common;

namespace BWR.Application.Dtos.Client
{
    public class ClientAttatchmentDto : EntityDto
    {
        public string Path { get; set; }
        public bool IsEnabled { get; set; }
        public int AttachmentId { get; set; }
        public int ClientId { get; set; }
    }
}