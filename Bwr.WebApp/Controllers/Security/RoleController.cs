using Bwr.WebApp.Identity;
using Bwr.WebApp.Models.Security;
using BWR.Application.Dtos.Role;
using BWR.Application.Interfaces.Security;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using BWR.ShareKernel.Permisions;
using DataTables.Mvc;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers
{
    public class RoleController : Controller
    {
        private readonly IRoleAppService _roleAppService;
        private readonly IPermissionAppService _permissionAppService;
        private string _message;
        private bool _success;

        public RoleController(IRoleAppService roleAppService,IPermissionAppService permissionAppService)
        {
            _roleAppService = roleAppService;
            _permissionAppService = permissionAppService;
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
                var roles = _roleAppService.GetAll().AsQueryable();

                var totalCount = roles.Count();

                // searching and sorting
                roles = SearchAndSort(requestModel, roles);
                var filteredCount = roles.Count();

                // Paging
                roles = roles.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, roles.ToList(), filteredCount, totalCount);
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
            return View("_CreateRole");
        }

        [HttpPost]
        public async Task<ActionResult> Create(RoleInsertDto dto)
        {
            var exist = false;

            if (_roleAppService.CheckIfExist(dto.Name, ""))
            {
                _success = false;
                _message = "اسم الدور موجود مسبقاً";
                exist = true;
            }
            else
            {

                var roleDto = _roleAppService.Insert(dto);
                if (roleDto != null)
                {
                    _success = true;
                }
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء إضافة بيانات الدور";
                }

            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var dto = _roleAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditRole", dto);

            return View("_EditRole", dto);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(RoleUpdateDto dto)
        {
            var exist = false;

            if (_roleAppService.CheckIfExist(dto.Name, ""))
            {
                _success = false;
                _message = "اسم الدور موجود مسبقاً";
                exist = true;
            }
            else
            {
                var roleDto = _roleAppService.Update(dto);
                if (roleDto != null)
                {
                    _success = true;
                }
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء إضافة بيانات الدور";
                }

            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(Guid id)
        {
            var dto = _roleAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteRole", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(RoleDto dto)
        {
            _roleAppService.Delete(dto.RoleId);

            return Content("success");
        }

        [HttpGet]
        public ActionResult AddPermissions(Guid id)
        {
            var dto = _roleAppService.GetById(id);
            return View("_Permissions", dto);
        }

        [HttpPost]
        public ActionResult AddPermissions(List<string> permissions , Guid roleId)
        {
            var permissionsDto = _permissionAppService.SavePermissions(roleId , permissions);
            if (permissionsDto.Any())
            {
                _success = true;
                //var roleDto = _roleAppService.GetById(roleId);

                //if (roleDto!=null && User.IsInRole(roleDto.Name))
                //    UpdatePermissions();

            }
            else
            {
                _success = false;
                _message = "حدثت مشكلة اثناء حفظ الصلاحيات";
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        private void UpdatePermissions()
        {
            var roles = _roleAppService.GetRolesForSpecificUser(User.Identity.Name).Select(x=>x.Name);
            
            if (roles.Any())
            {
                Session["UserPermissions"] = _permissionAppService.Get(x => roles.Contains(x.Role.Name)).Select(x => x.Name).ToList();
            }
            else
            {
                Session["UserPermissions"] = new List<string>();
            }
        }

        public ActionResult GetPermissions(Guid roleId)
        {
            var options = new List<string>();
            var values = new List<string>();

            var listPermissions = new List<PermissionsViewModel>();

            var appPermissions = typeof(AppPermision).GetFields(BindingFlags.Public | BindingFlags.Static |
                BindingFlags.FlattenHierarchy).
                Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();

            var appPermissionsAr = typeof(AppPermisionAr).GetFields(BindingFlags.Public | BindingFlags.Static |
                BindingFlags.FlattenHierarchy).
                Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();

            var permissions = _permissionAppService.GetForSpecificRole(roleId);

            foreach(var appPermission in appPermissions)
            {
                var arabicName = appPermission.Name;
                var arabicFieldInfo = appPermissionsAr.Where(x => x.Name == appPermission.Name).FirstOrDefault();
                if (arabicFieldInfo != null)
                    arabicName = arabicFieldInfo.GetValue(null).ToString();

                var permViewModel = new PermissionsViewModel()
                {
                    Option = appPermission.GetValue(null).ToString(),
                    OptionAr= arabicName,
                    IsExist = permissions.Any(x => x.Name == appPermission.GetValue(null).ToString())
                };
                listPermissions.Add(permViewModel);
            }

            return Json(new { Permissions = listPermissions }, JsonRequestBehavior.AllowGet);
        }

        private IQueryable<RoleDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<RoleDto> query)
        {
            // Apply filters
            if (requestModel.Search.Value != string.Empty)
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Name.Contains(value) 
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
    }
}