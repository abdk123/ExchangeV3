using System;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.PublicIncome
{
    public class PublicIncomeInsertDto
    {
        [Required(ErrorMessage = "اسم نوع الواردات مطلوب")]
        [Display(Name = "اسم نوع الواردات")]
        public string Name { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? Created { get; set; }
        
    }
}
