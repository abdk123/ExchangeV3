using BWR.Application.Dtos.Setting.PublicExpense;
using BWR.Application.Interfaces.Setting;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using DataTables.Mvc;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
namespace Bwr.WebApp.Controllers.Setting
{
    public class PublicExpenseController : Controller
    {
        private readonly IPublicExpenseAppService _publicExpenseAppService;
        private string _message;
        private bool _success;

        public PublicExpenseController(IPublicExpenseAppService publicExpenseAppService)
        {
            _publicExpenseAppService = publicExpenseAppService;

            _message = "";
            _success = false;
        }

        public ActionResult Index()
        {
            return View();
        }

        #region PublicExpense

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var publicExpenses = _publicExpenseAppService.GetAll().AsQueryable();

                var totalCount = publicExpenses.Count();

                // searching and sorting
                publicExpenses = SearchAndSort(requestModel, publicExpenses);
                var filteredCount = publicExpenses.Count();

                // Paging
                publicExpenses = publicExpenses.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, publicExpenses.ToList(), filteredCount, totalCount);
                return Json(dataTablesResponse, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetSpecificPublicExpense(string name, int? id)
        {

            try
            {
                var publicExpense = new PublicExpenseForDropdownDto();
                if (id != null)
                    publicExpense = _publicExpenseAppService.GetForDropdown(name).FirstOrDefault(x => x.Id != id);
                else
                    publicExpense = _publicExpenseAppService.GetForDropdown(name).FirstOrDefault();

                return Json(new { Success = true, PublicExpense = publicExpense }, JsonRequestBehavior.AllowGet);
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
            return View("_CreatePublicExpense");
        }

        [HttpPost]
        public ActionResult Create(PublicExpenseInsertDto dto)
        {
            var exist = false;

            if (_publicExpenseAppService.CheckIfExist(dto.Name, 0))
            {
                _success = false;
                _message = "يوجد بلد بنفس الاسم";
                exist = true;
            }
            else
            {
                var publicExpenseDto = _publicExpenseAppService.Insert(dto);
                if (publicExpenseDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء إضافة بيانات البلد";
                }
            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var dto = _publicExpenseAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditPublicExpense", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(PublicExpenseUpdateDto dto)
        {
            var exist = false;

            if (_publicExpenseAppService.CheckIfExist(dto.Name, dto.Id))
            {
                _success = false;
                _message = "يوجد بلد بنفس الاسم";
                exist = true;
            }
            else
            {
                var publicExpenseDto = _publicExpenseAppService.Update(dto);
                if (publicExpenseDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء تعديل بيانات البلد";
                }
            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var dto = _publicExpenseAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeletePublicExpense", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(PublicExpenseDto dto)
        {
            _publicExpenseAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        public ActionResult Detail(int id)
        {
            var dto = _publicExpenseAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DetailsPublicExpense", dto);

            return View(dto);
        }

        #endregion


        #region Helper Method

        private IQueryable<PublicExpenseDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<PublicExpenseDto> query)
        {
            // Apply filters
            if (requestModel.Search.Value != string.Empty)
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Name.Contains(value)
                                   //p.Address.Contains(value) 
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

        #endregion
    }
}