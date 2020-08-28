
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.Country
{
    public class CountryUpdateDto
    {
        public CountryUpdateDto()
        {
            Provinces = new List<CountryDto>();
        }
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم البلد مطلوب")]
        [Display(Name = "اسم البلد")]

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public int? MainCountryId { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? Modified { get; set; }
        public IList<CountryDto> Provinces { get; set; }
    }
}
