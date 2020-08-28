using BWR.Application.Common;

namespace BWR.Application.Dtos.Setting.Coin
{
    public class CoinDto : EntityDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string ISOCode { get; set; }
        public bool IsEnabled { get; set; }

    }
}
