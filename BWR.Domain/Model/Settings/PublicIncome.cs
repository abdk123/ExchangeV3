using BWR.ShareKernel.Common;

namespace BWR.Domain.Model.Settings
{
    public class PublicIncome : Entity
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }
}