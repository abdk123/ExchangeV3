using BWR.Domain.Model.Security;
using BWR.ShareKernel.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Action = BWR.Domain.Model.Settings.Action;

namespace BWR.Domain.Model.Logs
{
    public class Log: Entity
    {
        public string CloumnName { get; set; }
        public string BeforeUpdate { get; set; }
        public string AfterUpdate { get; set; }
        public DateTime Date { get; set; }
        public string Object { get; set; }

        public int ActionId { get; set; }
        [ForeignKey("ActionId")]
        public virtual Action Action { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
