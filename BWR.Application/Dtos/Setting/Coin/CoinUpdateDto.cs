
using BWR.Application.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.Coin
{
    public class CoinUpdateDto : EntityDto
    {
        [Display(Name = "اسم العملة")]
        [Required(ErrorMessage = "اسم العملة مطلوب")]
        public string Name { get; set; }
        
        [Display(Name = "رمز العملة")]
        [Required(ErrorMessage = "رمز العملة مطلوب")]
        public string Code { get; set; }

        public string ISOCode { get; set; }
        public bool IsEnabled { get; set; }
    }
}
