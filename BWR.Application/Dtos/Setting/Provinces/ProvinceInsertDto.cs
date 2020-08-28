using System;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.Provinces
{
    public class ProvinceInsertDto
    {
        [Required(ErrorMessage ="اسم المحافظة مطلوب")]
        [Display(Name = "اسم المحافظة")]
        public string Name { get; set; }

        [Required(ErrorMessage = "اسم البلد مطلوب")]
        [Display(Name = "اسم البلد")]
        public int? MainCountryId { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? Created { get; set; }
        
    }
}
