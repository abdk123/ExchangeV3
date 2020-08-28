
using BWR.Application.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Company
{
    public class CompanyUpdateDto : EntityDto
    {
        public CompanyUpdateDto()
        {
            CompanyCountries = new List<CompanyCountryDto>();
            CompanyCashes = new List<CompanyCashDto>();
        }

        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        [Display(Name = "اسم الشركة")]
        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        [Display(Name = "اخيار المناطق")]
        [Required(ErrorMessage = "يجب اختيار منطقة على الاقل")]
        public IList<CompanyCountryDto> CompanyCountries { get; set; }

        public IList<CompanyCashDto> CompanyCashes { get; set; }
    }
}
