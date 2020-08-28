using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Setting.Coin
{
    public class CoinInsertDto
    {

        [Required(ErrorMessage ="اسم العملة مطلوب")]
        [Display(Name = "اسم العملة")]
        public string Name { get; set; }

        [Required(ErrorMessage = "رمز العملة مطلوب")]
        [Display(Name = "رمز العملة")]
        [StringLength(3,ErrorMessage ="يجب أن يكون الكود مكون من ثلاث محارف")]
        public string Code { get; set; }

        public string ISOCode { get; set; }
        public bool IsEnabled { get; set; }

    }
}
