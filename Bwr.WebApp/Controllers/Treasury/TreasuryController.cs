using BWR.Application.Dtos.Treasury;
using BWR.Application.Interfaces.Treasury;
using BWR.Application.Interfaces.Setting;
using BWR.Infrastructure.Exceptions;
using BWR.ShareKernel.Exceptions;
using DataTables.Mvc;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers
{
    public class TreasuryController : Controller
    {
        private readonly ITreasuryAppService _treasuryAppService;
        private readonly ITreasuryCashAppService _treasuryCashAppService;
        private readonly ICoinAppService _coinAppService;
        private string _message;
        private bool _success;

        public TreasuryController(
            ITreasuryAppService treasuryAppService,
            ITreasuryCashAppService treasuryCashAppService,
            ICoinAppService coinAppService)
        {
            _treasuryAppService = treasuryAppService;
            _treasuryCashAppService = treasuryCashAppService;
            _coinAppService = coinAppService;

            _message = "";
            _success = false;
        }

        public ActionResult Index()
        {
            return View();

        }

        public ActionResult Test()
        {
            return View();
        }

        #region Treasury

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var treasuries = _treasuryAppService.GetAllWithBalances().AsQueryable();

                var totalCount = treasuries.Count();

                // searching and sorting
                treasuries = SearchAndSort(requestModel, treasuries);
                var filteredCount = treasuries.Count();

                // Paging
                treasuries = treasuries.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, treasuries.ToList(), filteredCount, totalCount);
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
            
            return View("_CreateTreasury");
        }

        [HttpPost]
        public ActionResult Create(TreasuryInsertDto dto)
        {
            var exist = false;
            if (!ModelState.IsValid)
            {

            }
            if (_treasuryAppService.CheckIfExist(dto.Name, 0))
            {
                _success = false;
                _message = "يوجد صندوق يمتلك نفس الأسم";
                exist = true;
            }
            else
            {
                var treasuryDto = _treasuryAppService.Insert(dto);
                if (treasuryDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء إضافة بيانات الصندوق";
                }
            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var dto = _treasuryAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditTreasury", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(TreasuryUpdateDto dto)
        {
            var exist = false;

            if (_treasuryAppService.CheckIfExist(dto.Name, dto.Id.Value))
            {
                _success = false;
                _message = "توجد صندوق بنفس الاسم";
                exist = true;
            }
            else
            {
                var treasuryDto = _treasuryAppService.Update(dto);
                if (treasuryDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء تعديل بيانات الصندوق";
                }
            }

            return Json(new { Success = _success, Message = _message, Exist = exist }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var dto = _treasuryAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteTreasury", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(TreasuryDto dto)
        {
            _treasuryAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        #endregion

        #region Treasury Details

        public ActionResult Detail(int id)
        {
            var dto = _treasuryAppService.GetById(id);

            var coins = _coinAppService.GetAll();
            ViewBag.Coins = new SelectList(coins, "Id", "Name");

            return View("_DetailsTreasury", dto);
        }


        #endregion

        

        #region Helper Method

        private IQueryable<TreasurysDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<TreasurysDto> query)
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

        #endregion
    }
}