
using BWR.Application.Dtos.Setting.Country;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.PublicIncome
{ 
    public class PublicIncomeUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم نوع الواردات مطلوب")]
        [Display(Name = "اسم نوع الواردات")]
        public string Name { get; set; }

        public bool IsEnabled { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? Modified { get; set; }
        
    }
}
