using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Treasury
{
    public class TreasuryInsertDto
    {
        public TreasuryInsertDto()
        {
            TreasuryCashes = new List<TreasuryCashDto>();
        }

        [Required(ErrorMessage = "اسم الصندوق مطلوب")]
        [Display(Name = "اسم الصندوق")]
        public string Name { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsAvilable { get; set; }

        public IList<TreasuryCashDto> TreasuryCashes { get; set; }
    }
}
