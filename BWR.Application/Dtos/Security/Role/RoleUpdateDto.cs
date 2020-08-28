using System;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Role
{
    public class RoleUpdateDto 
    {
         public Guid RoleId { get; set; }

        [Required(ErrorMessage = "«·«”„ „ÿ·Ê»")]
        [Display(Name = "«”„ «·œÊ—")]
        public string Name { get; set; }
        
    }
}
