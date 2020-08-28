using BWR.Domain.Model.Settings;
using BWR.ShareKernel.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Companies
{
    public class CompanyUser: Entity
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
    }
}