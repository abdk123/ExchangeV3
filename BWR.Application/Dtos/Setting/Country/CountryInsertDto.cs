using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.Country
{
    public class CountryInsertDto
    {
        public CountryInsertDto()
        {
            Provinces = new List<CountryDto>();
        }

        [Required(ErrorMessage ="اسم البلد مطلوب")]
        [Display(Name = "اسم البلد")]
        public string Name { get; set; }
        
        public string CreatedBy { get; set; }
        public DateTime? Created { get; set; }
        public bool IsDeleted{ get; set; }
        public IList<CountryDto> Provinces { get; set; }
    }
}
