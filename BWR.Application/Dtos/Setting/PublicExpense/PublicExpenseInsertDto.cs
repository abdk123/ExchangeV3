using System;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.PublicExpense
{
    public class PublicExpenseInsertDto
    {
        [Required(ErrorMessage = "اسم نوع النفقات مطلوب")]
        [Display(Name = "اسم نوع النفقات")]
        public string Name { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? Created { get; set; }
        
    }
}
