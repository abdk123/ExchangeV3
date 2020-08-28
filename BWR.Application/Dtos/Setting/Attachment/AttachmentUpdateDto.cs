
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.Attachment
{ 
    public class AttachmentUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المرفق")]
        [Display(Name = "اسم المرفق")]
        public string Name { get; set; }

        public bool IsEnabled { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? Modified { get; set; }
        
    }
}
