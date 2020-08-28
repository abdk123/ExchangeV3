using BWR.ShareKernel.Common;

namespace BWR.Domain.Model.Settings
{
    public class Coin: Entity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string ISOCode { get; set; }
        public bool IsEnabled { get; set; }
    }
}
