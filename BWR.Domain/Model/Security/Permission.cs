using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BWR.Domain.Model.Security
{
    [Table("Permissions")]
    public class Permission 
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public Role Role { get; set; }

        public string GrantedByUser { get; set; }

        public DateTime? GrantedDate { get; set; }
    }
}
