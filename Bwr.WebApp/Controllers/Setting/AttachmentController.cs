using System.Linq;
using System.Web.Mvc;
using DataTables.Mvc;
using System.Linq.Dynamic;
using BWR.ShareKernel.Exceptions;
using BWR.Infrastructure.Exceptions;
using BWR.Application.Dtos.Setting.Attachment;
using BWR.Application.Interfaces.Setting;

namespace Bwr.WebApp.Controllers.Setting
{
    public class AttachmentController : Controller
    {
        private readonly IAttachmentAppService _attachmentAppService;
        private string _message;
        private bool _success;

        public AttachmentController(IAttachmentAppService attachmentAppService)
        {
            _attachmentAppService = attachmentAppService;

            _message = "";
            _success = false;
        }

        public ActionResult Index()
        {
            return View();
        }

        #region Attachment

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var attachments = _attachmentAppService.GetAll().AsQueryable();

                var totalCount = attachments.Count();

                // searching and sorting
                attachments = SearchAndSort(requestModel, attachments);
                var filteredCount = attachments.Count();

                // Paging
                attachments = attachments.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, attachments.ToList(), filteredCount, totalCount);
                return Json(dataTablesResponse, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetSpecificAttachment(string name, int? id)
        {

            try
            {
                var attachment = new AttachmentForDropdownDto();
                if (id != null)
                    attachment = _attachmentAppService.GetForDropdown(name).FirstOrDefault(x => x.Id != id);
                else
                    attachment = _attachmentAppService.GetForDropdown(name).FirstOrDefault();

                return Json(new { Success = true, Attachment = attachment }, JsonRequestBehavior.AllowGet);
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
            return View("_CreateAttachment");
        }

        [HttpPost]
        public ActionResult Create(AttachmentInsertDto dto)
        {
            var exist = false;

            if (_attachmentAppService.CheckIfExist(dto.Name, 0))
            {
                _success = false;
                _message = "يوجد بلد بنفس الاسم";
                exist = true;
            }
            else
            {
                var attachmentDto = _attachmentAppService.Insert(dto);
                if (attachmentDto != null)
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
            var dto = _attachmentAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditAttachment", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(AttachmentUpdateDto dto)
        {
            var exist = false;

            if (_attachmentAppService.CheckIfExist(dto.Name, dto.Id))
            {
                _success = false;
                _message = "يوجد بلد بنفس الاسم";
                exist = true;
            }
            else
            {
                var attachmentDto = _attachmentAppService.Update(dto);
                if (attachmentDto != null)
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
            var dto = _attachmentAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteAttachment", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(AttachmentDto dto)
        {
            _attachmentAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        public ActionResult Detail(int id)
        {
            var dto = _attachmentAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DetailsAttachment", dto);

            return View(dto);
        }

        #endregion


        #region Helper Method

        private IQueryable<AttachmentDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<AttachmentDto> query)
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