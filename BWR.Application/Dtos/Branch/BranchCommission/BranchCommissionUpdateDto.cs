using System.ComponentModel.DataAnnotations;
using BWR.Application.Common;

namespace BWR.Application.Dtos.Branch
{
    public class BranchCommissionUpdateDto : EntityDto
    {
        [Required(ErrorMessage = "Õﬁ· „‰ „ÿ·Ê»")]
        [Display(Name = "„‰")]
        public decimal StartRange { get; set; }

        [Required(ErrorMessage = "Õﬁ· ≈·Ï „ÿ·Ê»")]
        [Display(Name = "≈·Ï")]
        public decimal EndRange { get; set; }

        [Display(Name = "«·ﬁÌ„…")]
        public decimal? Cost { get; set; }

        [Display(Name = "«·ﬁÌ„…")]
        public decimal? Ratio { get; set; }

        public bool IsEnabled { get; set; }
        public int BranchId { get; set; }

        [Required(ErrorMessage = "«”„ «·⁄„·… „ÿ·Ê»")]
        [Display(Name = "«”„ «·⁄„·…")]
        public int CoinId { get; set; }

        [Required(ErrorMessage = "«”„ «·»·œ „ÿ·Ê»")]
        [Display(Name = "«”„ «·»·œ")]
        public int CountryId { get; set; }
    }
}
