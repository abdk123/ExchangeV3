using BWR.Application.Dtos.Setting.Attachment;
using System.Collections.Generic;

namespace BWR.Application.Interfaces.Setting
{
    public interface IAttachmentAppService : IGrudAppService<AttachmentDto, AttachmentInsertDto, AttachmentUpdateDto>
    {
        IList<AttachmentForDropdownDto> GetForDropdown(string name);

        bool CheckIfExist(string name, int id);
    }
}
