
using BWR.Application.Dtos.Setting.Country;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.Provinces
{ 
    public class ProvinceUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المحافظة مطلوب")]
        [Display(Name = "اسم المحافظة")]
        public string Name { get; set; }

        [Required(ErrorMessage = "اسم البلد مطلوب")]
        [Display(Name = "اسم البلد")]
        public int? MainCountryId { get; set; }

        public bool IsEnabled { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? Modified { get; set; }
        
    }
}
