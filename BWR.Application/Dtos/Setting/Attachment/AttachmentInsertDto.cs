using System;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.Attachment
{
    public class AttachmentInsertDto
    {
        [Required(ErrorMessage = "اسم المرفق")]
        [Display(Name = "اسم المرفق")]
        public string Name { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? Created { get; set; }
        
    }
}
