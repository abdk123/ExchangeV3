
using BWR.Application.Dtos.Setting.Country;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.PublicExpense
{ 
    public class PublicExpenseUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم نوع النفقات مطلوب")]
        [Display(Name = "اسم نوع النفقات")]
        public string Name { get; set; }

        public bool IsEnabled { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? Modified { get; set; }
        
    }
}
