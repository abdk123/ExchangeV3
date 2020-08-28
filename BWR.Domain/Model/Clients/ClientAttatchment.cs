using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Clients
{
    [Table("BwClientAttatchment")]
    public class ClientAttatchment: Entity
    {
        public string Path { get; set; }
        public bool IsEnabled { get; set; }

        public int AttachmentId { get; set; }

        [ForeignKey("AttachmentId")]
        public virtual Attachment Attatchmant { get; set; }

        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }
    }
}
