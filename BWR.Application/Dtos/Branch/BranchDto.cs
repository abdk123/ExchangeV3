using BWR.Application.Common;

namespace BWR.Application.Dtos.Branch
{
    public class BranchDto : EntityDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}