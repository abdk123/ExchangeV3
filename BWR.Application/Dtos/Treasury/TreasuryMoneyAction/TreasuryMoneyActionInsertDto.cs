using BWR.Application.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Treasury.TreasuryMoneyAction
{
    public class TreasuryMoneyActionInsertDto: EntityDto
    {
        public int TreasuryId { get; set; }

        [Required(ErrorMessage = "العملة مطلوبة")]
        [Display(Name = "العملة")]
        public int CoinId { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "نوع الحركة مطلوبة")]
        [Display(Name = "نوع الحركة")]
        public int? ActionType { get; set; }
    }
}
