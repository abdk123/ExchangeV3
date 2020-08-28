using BWR.Application.Dtos.Setting.PublicIncome;
using BWR.Application.Interfaces.Setting;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using DataTables.Mvc;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Setting
{
    public class PublicIncomeController : Controller
    {
        private readonly IPublicIncomeAppService _publicIncomeAppService;
        private string _message;
        private bool _success;

        public PublicIncomeController(IPublicIncomeAppService publicIncomeAppService)
        {
            _publicIncomeAppService = publicIncomeAppService;
            
            _message = "";
            _success = false;
        }

        public ActionResult Index()
        {
            return View();
        }

        #region PublicIncome

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var publicIncomes = _publicIncomeAppService.GetAll().AsQueryable();

                var totalCount = publicIncomes.Count();

                // searching and sorting
                publicIncomes = SearchAndSort(requestModel, publicIncomes);
                var filteredCount = publicIncomes.Count();

                // Paging
                publicIncomes = publicIncomes.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, publicIncomes.ToList(), filteredCount, totalCount);
                return Json(dataTablesResponse, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetSpecificPublicIncome(string name, int? id)
        {

            try
            {
                var publicIncome = new PublicIncomeForDropdownDto();
                if (id != null)
                    publicIncome = _publicIncomeAppService.GetForDropdown(name).FirstOrDefault(x => x.Id != id);
                else
                    publicIncome = _publicIncomeAppService.GetForDropdown(name).FirstOrDefault();

                return Json(new { Success = true, PublicIncome = publicIncome }, JsonRequestBehavior.AllowGet);
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
            return View("_CreatePublicIncome");
        }

        [HttpPost]
        public ActionResult Create(PublicIncomeInsertDto dto)
        {
            var exist = false;

            if (_publicIncomeAppService.CheckIfExist(dto.Name, 0))
            {
                _success = false;
                _message = "يوجد بلد بنفس الاسم";
                exist = true;
            }
            else
            {
                var publicIncomeDto = _publicIncomeAppService.Insert(dto);
                if (publicIncomeDto != null)
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
            var dto = _publicIncomeAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditPublicIncome", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(PublicIncomeUpdateDto dto)
        {
            var exist = false;

            if (_publicIncomeAppService.CheckIfExist(dto.Name, dto.Id))
            {
                _success = false;
                _message = "يوجد بلد بنفس الاسم";
                exist = true;
            }
            else
            {
                var publicIncomeDto = _publicIncomeAppService.Update(dto);
                if (publicIncomeDto != null)
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
            var dto = _publicIncomeAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeletePublicIncome", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(PublicIncomeDto dto)
        {
            _publicIncomeAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        public ActionResult Detail(int id)
        {
            var dto = _publicIncomeAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DetailsPublicIncome", dto);

            return View(dto);
        }

        #endregion


        #region Helper Method

        private IQueryable<PublicIncomeDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<PublicIncomeDto> query)
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