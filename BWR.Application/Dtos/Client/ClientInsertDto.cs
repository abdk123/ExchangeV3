using BWR.Domain.Model.Clients;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BWR.Application.Dtos.Client
{
    public class ClientInsertDto
    {
        public ClientInsertDto()
        {
            ClientPhones = new List<ClientPhoneDto>();
            ClientCashes = new List<ClientCashDto>();
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

        public ClientType ClientType { get; set; }

        public IList<ClientPhoneDto> ClientPhones { get; set; }
        public IList<ClientCashDto> ClientCashes { get; set; }
    }
}
