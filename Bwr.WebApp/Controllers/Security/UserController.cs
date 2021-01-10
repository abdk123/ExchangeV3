using Bwr.WebApp.Identity;
using Bwr.WebApp.Models.Security;
using BWR.Application.Dtos.Role;
using BWR.Application.Dtos.User;
using BWR.Application.Interfaces.Security;
using BWR.Application.Interfaces.Treasury;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using DataTables.Mvc;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserAppService _userAppService;
        private readonly ITreasuryAppService _treasuryAppService;
        private readonly IRoleAppService _roleAppService;
        private readonly UserManager<IdentityUser, Guid> _userManager;
        
        private string _message;
        private bool _success;

        public UserController(
            IUserAppService userAppService,
            UserManager<IdentityUser, Guid> userManager,
            ITreasuryAppService treasuryAppService,
            IRoleAppService roleAppService)
        {
            _userAppService = userAppService;
            _userManager = userManager;
            _treasuryAppService = treasuryAppService;
            _roleAppService = roleAppService;
            _message = "";
            _success = false;
            
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var users = _userAppService.GetAll(c=>c.UserName!= "admin").AsQueryable();

                var totalCount = users.Count();

                // searching and sorting
                users = SearchAndSort(requestModel, users);
                var filteredCount = users.Count();

                // Paging
                users = users.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, users.ToList(), filteredCount, totalCount);
                return Json(dataTablesResponse, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View("_CreateUser");
        }

        [HttpPost]
        public async Task<ActionResult> Create(UserInsertDto dto)
        {
            var exist = false;

            if (_userAppService.CheckIfExist(dto.Username, ""))
            {
                _success = false;
                _message = "اسم المستخدم موجود مسبقاً";
                exist = true;
            }
            else
            {

                var user = new IdentityUser() { UserName = dto.Username, FullName = dto.FullName };
                var result = await _userManager.CreateAsync(user, dto.PasswordHash);
                if (result.Succeeded)
                {
                    _success = true;
                }
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء إضافة بيانات المستخدم";
                }

            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var dto = _userAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditUser",dto);

            return View("_EditUser",dto);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(UserUpdateDto dto)
        {
            var exist = false;

            if (_userAppService.CheckIfExist(dto.Username, dto.UserId.ToString()))
            {
                _success = false;
                _message = "اسم المستخدم موجود مسبقاً";
                exist = true;
            }
            else
            {
                var updateDto = _userAppService.GetForEdit(dto.UserId);
                IdentityUser user = null;

                user = new IdentityUser(dto.Username) { Id = dto.UserId, UserName = dto.Username, FullName = dto.FullName };

                var result = new IdentityResult();
                if (!string.IsNullOrEmpty(dto.PasswordHash))
                {
                    var password = _userManager.PasswordHasher.HashPassword(dto.PasswordHash);
                    user.PasswordHash = password;
                    await _userManager.UpdateAsync(user);
                    result = await _userManager.UpdateSecurityStampAsync(user.Id);
                }
                else
                {
                    result = await _userManager.UpdateAsync(user);
                }

                if (result.Succeeded)
                {
                    _success = true;
                }
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء إضافة بيانات المستخدم";
                }

            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(Guid id)
        {
            var dto = _userAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteUser", dto);

            return View(dto);
        }

        public ActionResult Detail(Guid id)
        {
            var dto = _userAppService.GetUserWithTreasuries(id);
            ViewBag.AvilableTreasury = _treasuryAppService.GetAll().Where(c => c.IsAvilable == true).ToList();
            return View("_DetailsUser", dto);
        }

        [HttpPost]
        public ActionResult Delete(UserDto dto)
        {
            _userAppService.Delete(dto.UserId);

            return Content("success");
        }

        [HttpGet]
        public ActionResult AssignRolesToUser(Guid id)
        {
            var dto = _userAppService.GetById(id);
            return View("_AssignRolesToUser", dto);
        }

        [HttpPost]
        public async Task<ActionResult> AssignRolesToUser(IList<RoleDto> roles, Guid userId)
        {
            try
            {
                var identityUser = await _userManager.FindByIdAsync(userId);
                await CheckForDeleteRoleAsync(roles, identityUser);
                await CheckForAddRoleAsync(roles, identityUser);
                _success = true;
            }
            catch(Exception ex)
            {
                _success = false;
                _message = "حدثت مشكلة اثناء الإسناد";
            }
            

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRoles(Guid userId)
        {
            var userRoles = _roleAppService.GetRolesForSpecificUser(userId);
            var roles = _roleAppService.GetAll();
            var list = new List<UserRolesViewModel>();
            foreach (var role in roles)
            {
                var userRolesViewModel = new UserRolesViewModel()
                {
                    Name = role.Name,
                    RoleId=role.RoleId,
                    IsExist = userRoles.Any(x => x.Name == role.Name)
                };

                list.Add(userRolesViewModel);
            }

            return Json(new { Roles = list }, JsonRequestBehavior.AllowGet);
        }

        private IQueryable<UserDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<UserDto> query)
        {
            // Apply filters
            if (requestModel.Search.Value != string.Empty)
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p => p.FullName.Contains(value) ||
                                   p.Username.Contains(value)

                                   );
            }

            var filteredCount = query.Count();

            // Sort
            var sortedColumns = requestModel.Columns.GetSortedColumns();
            var orderByString = string.Empty;

            foreach (var column in sortedColumns)
            {
                orderByString += orderByString != string.Empty ? "," : "";
                orderByString += (column.Data) + (column.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
            }

            query = query.OrderBy(orderByString == string.Empty ? "BarCode asc" : orderByString);

            return query;
        }

        public ActionResult GiveTreasury(Guid userId, int treasuryId)
        {
            try
            {
                _userAppService.GiveTreasury(userId, treasuryId);
                return RedirectToAction("Detail", new { id = userId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Message", new { message = "حدث خطاء ما" });
            }
        }
        [HttpPost]
        public ActionResult ReceiveTreasury(int userTreaseryId, Guid userId)
        {
            try
            {
                _userAppService.ReceiveTreasury(userTreaseryId, userId);
                return RedirectToAction("Detail", new { id = userId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Message", new { message = "حدث خطاء ما" });
            }
        }

        private async Task CheckForDeleteRoleAsync(IList<RoleDto> rolesDto, IdentityUser identityUser)
        {
            var rolesMustDelete = new List<RoleDto>();
            if (rolesDto == null || !rolesDto.Any())
            {
                rolesMustDelete = _roleAppService.GetRolesForSpecificUser(identityUser.Id).ToList();
            }
            else
            {
                rolesMustDelete = _roleAppService.GetRolesForSpecificUser(identityUser.Id)
                    .Where(x => !rolesDto.Select(y => y.Name).Contains(x.Name)).ToList();
            }

            foreach (var roleMustDelete in rolesMustDelete)
            {
               await _userManager.RemoveFromRoleAsync(identityUser.Id, roleMustDelete.Name);
            }
            
        }

        private async Task CheckForAddRoleAsync(IList<RoleDto> rolesDto, IdentityUser identityUser)
        {
            if (rolesDto != null)
            {
                foreach (var roleDto in rolesDto)
                {
                    var roles = _roleAppService.GetRolesForSpecificUser(identityUser.Id).Where(x => x.Name == roleDto.Name);
                    if (roles.Count() == 0 || roles == null)
                    {
                        await _userManager.AddToRoleAsync(identityUser.Id, roleDto.Name);
                    }
                }
                
            }

        }
    }
}