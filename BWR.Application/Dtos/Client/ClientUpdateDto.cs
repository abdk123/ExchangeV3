using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BWR.Application.Common;

namespace BWR.Application.Dtos.Client
{
    public class ClientUpdateDto: EntityDto
    {
        public ClientUpdateDto()
        {
            ClientPhones = new List<ClientPhoneDto>();
            ClientCashes = new List<ClientCashDto>();
            ClientAttatchments = new List<ClientAttatchmentDto>();
        }

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [Display(Name = "اسم العميل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "عنوان العميل مطلوب")]
        [Display(Name = "عنوان العميل")]
        public string Address { get; set; }

        [Display(Name = "الحساب فعال")]
        public bool IsEnabled { get; set; }

        [Display(Name = "البلد")]
        [Required(ErrorMessage = "اسم البلد مطلوب")]
        public int? CountryId { get; set; }

        public IList<ClientPhoneDto> ClientPhones { get; set; }
        public IList<ClientCashDto> ClientCashes { get; set; }
        public IList<ClientAttatchmentDto> ClientAttatchments { get; set; }
    }
}
