using System;
using System.Collections.Generic;
using BWR.Application.Dtos.Security.User;
using BWR.Application.Dtos.Treasury.UserTreasury;
using BWR.Application.Dtos.User;
namespace BWR.Application.Interfaces.Security
{
    public interface IUserAppService 
    {
        IList<UserForDropdownDto> GetForDropdown(string name);
        bool CheckIfExist(string name, string id);
        IList<UserDto> GetAll();
        UserDto GetById(Guid id);
        UserDto Insert(UserInsertDto dto);
        UserDto Update(UserUpdateDto dto);
        UserUpdateDto GetForEdit(Guid id);
        UserDetailDto GetUserWithTreasuries(Guid id);
        UserDetailDto GiveTreasury(Guid userId, int treasuryId);
        UserDetailDto ReceiveTreasury(int userTreaseryId, Guid userId);
        void Delete(Guid id);
        
    }
}
