using System;
using System.ComponentModel.DataAnnotations;

namespace BWR.ShareKernel.Common
{
    public class Entity
    {
        [Key]
        public int Id { get; set; }
        
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public bool IsDeleted { get; set; }
    }
}
