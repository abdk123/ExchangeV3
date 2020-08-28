using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using BWR.Application.Dtos.Branch;
using BWR.Application.Dtos.Branch.BranchCommission;
using BWR.Application.Interfaces.Branch;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using DataTables.Mvc;

namespace Bwr.WebApp.Controllers
{
    public class BranchCommissionController : Controller
    {
        private readonly IBranchCommissionAppService _branchCommissionAppService;
        private string _message;
        private bool _success;

        public BranchCommissionController(IBranchCommissionAppService branchCommissionAppService)
        {
            _branchCommissionAppService = branchCommissionAppService;
            _message = "";
            _success = false;
        }

        public ActionResult Index()
        {
            return View();
        }

        #region BranchCommission

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var branchCommissions = _branchCommissionAppService.GetAll().AsQueryable();

                var totalCount = branchCommissions.Count();

                // searching and sorting
                branchCommissions = SearchAndSort(requestModel, branchCommissions);
                var filteredCount = branchCommissions.Count();

                // Paging
                branchCommissions = branchCommissions.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, branchCommissions.ToList(), filteredCount, totalCount);
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
            return View("_CreateBranchCommission");
        }

        [HttpPost]
        public ActionResult Create(BranchCommissionInsertDto dto, string commissionType)
        {
            //في حال تغيير النوع يجب تصفير القيمة الاخرى
            if (commissionType == "Cost")
            {
                dto.Ratio = 0;
            }
            else if (commissionType == "Ratio")
            {
                dto.Cost = 0;
            }
            var branchCommissionDto = _branchCommissionAppService.Insert(dto);
            if (branchCommissionDto != null)
                _success = true;
            else
            {
                _success = false;
                _message = "حدثت مشكلة اثناء إضافة البيانات ";
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var dto = _branchCommissionAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditBranchCommission", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(BranchCommissionUpdateDto dto, string commissionType)
        {
            //في حال تغيير النوع يجب تصفير القيمة الاخرى
            if (commissionType == "Cost")
            {
                dto.Ratio = 0;
            }
            else if (commissionType == "Ratio")
            {
                dto.Cost = 0;
            }

            var branchCommissionDto = _branchCommissionAppService.Update(dto);
            if (branchCommissionDto != null)
                _success = true;
            else
            {
                _success = false;
                _message = "حدثت مشكلة اثناء تعديل البيانات ";
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var dto = _branchCommissionAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteBranchCommission", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(BranchCommissionDto dto)
        {
            _branchCommissionAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        public ActionResult Detail(int id)
        {
            var dto = _branchCommissionAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DetailsBranchCommission", dto);

            return View(dto);
        }

        #endregion

        public decimal? CalcComission(int countryId, int coinId, decimal amount, int? branchId)
        {
            if (branchId == null)
                branchId = BranchHelper.Id;

            var input = new BranchCommissionInputDto()
            {
                Amount = amount,
                BranchId = branchId,
                CoinId = coinId,
                CountryId = countryId
            };

            return _branchCommissionAppService.CalcComission(input);
        }

        #region Helper Method

        private IQueryable<BranchCommissionDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<BranchCommissionDto> query)
        {
            // Apply filters
            if (requestModel.Search.Value != string.Empty)
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Country.Name.Contains(value) ||
                                   p.Coin.Name.Contains(value) ||
                                   p.EndRange.Equals(value) ||
                                   p.StartRange.Equals(value) ||
                                   p.Ratio.Equals(value) ||
                                   p.Cost.Equals(value)
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
