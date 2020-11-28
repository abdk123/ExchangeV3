using AutoMapper;
using BWR.Application.Dtos.Security.User;
using BWR.Application.Dtos.Treasury.UserTreasury;
using BWR.Application.Dtos.User;
using BWR.Application.Interfaces.Security;
using BWR.Application.Interfaces.Shared;
using BWR.Domain.Model.Security;
using BWR.Domain.Model.Treasures;
using BWR.Infrastructure.Context;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BWR.Application.AppServices.Security
{
    public class UserAppService : IUserAppService
    {
        private readonly IUnitOfWork<MainContext> _unitOfWork;
        private readonly IAppSession _appSession;

        public UserAppService(IUnitOfWork<MainContext> unitOfWork, IAppSession appSession)
        {
            _unitOfWork = unitOfWork;
            _appSession = appSession;
        }

        public IList<UserDto> GetAll()
        {
            var usersDtos = new List<UserDto>();
            try
            {
                var users = _unitOfWork.GenericRepository<User>().GetAll().ToList();
                usersDtos = Mapper.Map<List<User>, List<UserDto>>(users);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return usersDtos;
        }
        public IList<UserDto> GetAll(Expression<Func<User, bool>> predicate)
        {
            var usersDtos = new List<UserDto>();
            try
            {
                var users = _unitOfWork.GenericRepository<User>().FindBy(predicate).ToList();
                usersDtos = Mapper.Map<List<User>, List<UserDto>>(users);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return usersDtos;
        }

        public UserDto GetById(Guid id)
        {
            UserDto userDto = null;
            try
            {
                var user = _unitOfWork.GenericRepository<User>().GetById(id);
                if (user != null)
                {
                    userDto = Mapper.Map<User, UserDto>(user);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return userDto;
        }

        public IList<UserForDropdownDto> GetForDropdown(string name)
        {
            var usersDtos = new List<UserForDropdownDto>();
            try
            {
                var users = _unitOfWork.GenericRepository<User>().FindBy(x => x.UserName.StartsWith(name)).ToList();
                Mapper.Map<List<User>, List<UserForDropdownDto>>(users, usersDtos);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return usersDtos;
        }

        public UserUpdateDto GetForEdit(Guid id)
        {
            UserUpdateDto userDto = null;
            try
            {
                var user = _unitOfWork.GenericRepository<User>().GetById(id);
                if (user != null)
                {
                    userDto = Mapper.Map<User, UserUpdateDto>(user);
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return userDto;
        }

        public UserDetailDto GetUserWithTreasuries(Guid id)
        {
            UserDetailDto userDto = null;
            try
            {
                var user = _unitOfWork.GenericRepository<User>().GetById(id);
                if (user != null)
                {
                    userDto = Mapper.Map<User, UserDetailDto>(user);

                    var userTreasuries = _unitOfWork.GenericRepository<UserTreasuery>().FindBy(x => x.UserId == id).ToList();

                    foreach(var userTreasury in userTreasuries)
                    {
                        var userTreasuryDto = new UserTreasuryDto()
                        {
                            Id = userTreasury.Id,
                            UserId = userDto.UserId,
                            DeliveryDate = userTreasury.DeliveryDate,
                            TreasuryId = userTreasury.TreasuryId,
                            Created= userTreasury.Created,
                            Modified=userTreasury.Modified,
                            TreasuryName = userTreasury.Treasury != null ? userTreasury.Treasury.Name : string.Empty
                        };

                        userDto.UserTreasuries.Add(userTreasuryDto);
                    }
                }

            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return userDto;
        }

        public UserDto Insert(UserInsertDto dto)
        {
            UserDto userDto = null;
            try
            {
                var user = Mapper.Map<UserInsertDto, User>(dto);
                
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<User>().Insert(user);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                userDto = Mapper.Map<User, UserDto>(user);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
                _unitOfWork.Rollback();
            }
            return userDto;
        }

        public UserDto Update(UserUpdateDto dto)
        {
            UserDto userDto = null;
            try
            {
                var user = _unitOfWork.GenericRepository<User>().GetById(dto.UserId);
                Mapper.Map<UserUpdateDto, User>(dto, user);
                //user.ModifiedBy = _appSession.GetUserName();
                _unitOfWork.CreateTransaction();

                _unitOfWork.GenericRepository<User>().Update(user);
                _unitOfWork.Save();

                _unitOfWork.Commit();

                userDto = Mapper.Map<User, UserDto>(user);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return userDto;
        }

        public void Delete(Guid id)
        {
            try
            {
                var user = _unitOfWork.GenericRepository<User>().GetById(id);
                if (user != null)
                {
                    _unitOfWork.GenericRepository<User>().Delete(user);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
        }

        public bool CheckIfExist(string userName, string id)
        {
            try
            {
                var user = _unitOfWork.GenericRepository<User>()
                    .FindBy(x => x.UserName.Trim().Equals(userName.Trim()) && x.UserId.ToString() != id)
                    .FirstOrDefault();
                if (user != null)
                    return true;
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return false;
        }

        public UserDetailDto GiveTreasury(Guid userId, int treasuryId)
        {
            UserDetailDto dto = null;
            try
            {
                _unitOfWork.CreateTransaction();

                var userTreasury = _unitOfWork.GenericRepository<UserTreasuery>().FindBy(x => x.UserId==userId && x.DeliveryDate == null).LastOrDefault();
                if (userTreasury != null)
                {
                    userTreasury.DeliveryDate = DateTime.Now;
                    userTreasury.Modified = DateTime.Now;
                    userTreasury.ModifiedBy = _appSession.GetUserName();
                    _unitOfWork.GenericRepository<UserTreasuery>().Update(userTreasury);

                }

                var treusery = _unitOfWork.GenericRepository<Treasury>().FindBy(x => x.Id == treasuryId).FirstOrDefault();
                if (treusery != null)
                {
                    treusery.IsAvilable = false;
                    _unitOfWork.GenericRepository<Treasury>().Update(treusery);
                    var newUserTreasery = new UserTreasuery()
                    {
                        UserId = userId,
                        TreasuryId = treasuryId,
                        Created=DateTime.Now,
                        CreatedBy=_appSession.GetUserName()
                    };

                    _unitOfWork.GenericRepository<UserTreasuery>().Insert(newUserTreasery);
                }

                _unitOfWork.Save();
                _unitOfWork.Commit();

                dto = GetUserWithTreasuries(userId);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }

            return dto;
        }

        public UserDetailDto ReceiveTreasury(int userTreasuryId, Guid userId)
        {
            UserDetailDto dto = null;
            try
            {
                _unitOfWork.CreateTransaction();

                var userTreasury = _unitOfWork.GenericRepository<UserTreasuery>().FindBy(c => c.Id == userTreasuryId).FirstOrDefault();
                if (userTreasury != null)
                {
                    userTreasury.DeliveryDate = DateTime.Now;
                    userTreasury.Modified = DateTime.Now;
                    userTreasury.ModifiedBy = _appSession.GetUserName();
                    _unitOfWork.GenericRepository<UserTreasuery>().Update(userTreasury);
                }

                var treusery = _unitOfWork.GenericRepository<Treasury>().FindBy(x => x.Id == userTreasury.TreasuryId).FirstOrDefault();
                if (treusery != null)
                {
                    treusery.IsAvilable = true;
                    _unitOfWork.GenericRepository<Treasury>().Update(treusery);
                   
                }

                _unitOfWork.Save();
                _unitOfWork.Commit();

                dto = GetUserWithTreasuries(userId);
            }
            catch (Exception ex)
            {
                Tracing.SaveException(ex);
            }
            return dto;
        }

        
    }
}
