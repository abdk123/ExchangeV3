using BWR.Application.Interfaces.Setting;
using BWR.ShareKernel.Exceptions;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using BWR.Infrastructure.Exceptions;
using DataTables.Mvc;
using BWR.Application.Dtos.Client;
using BWR.Application.Interfaces.Client;
using System.Collections.Generic;
using AutoMapper;
using BWR.Domain.Model.Clients;
using System;
using BWR.Application.Interfaces.Shared;
using System.IO;
using BWR.Application.Extensions;

namespace Bwr.WebApp.Controllers
{
    public class ClientController : Controller
    {
        private readonly IClientAppService _clientAppService;
        private readonly IClientAttatchmentAppService _clientAttatchmentAppService;
        private readonly IProvinceAppService _provinceAppService;
        private readonly IAppSession _appSession;
        private string _message;
        private bool _success;

        public ClientController(
            IClientAppService clientAppService,
            IProvinceAppService provinceAppService,
            IClientAttatchmentAppService clientAttatchmentAppService,
            IAppSession appSession)
        {
            _clientAppService = clientAppService;
            _provinceAppService = provinceAppService;
            _clientAttatchmentAppService = clientAttatchmentAppService;
            _appSession = appSession;
            _message = "";
            _success = false;
        }

        public ActionResult Index()
        {
            return View();
        }

        #region Client

        public ActionResult Get([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            try
            {
                var clients = _clientAppService.GetAll().AsQueryable();

                var totalCount = clients.Count();

                // searching and sorting
                clients = SearchAndSort(requestModel, clients);
                var filteredCount = clients.Count();

                // Paging
                clients = clients.Skip(requestModel.Start).Take(requestModel.Length);

                var dataTablesResponse = new DataTablesResponse(requestModel.Draw, clients.ToList(), filteredCount, totalCount);
                return Json(dataTablesResponse, JsonRequestBehavior.AllowGet);
            }
            catch (BwrException ex)
            {
                Tracing.SaveException(ex);
                return Json(new { Success = false, Message = _message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetClientById(int clientId)
        {
            var client = _clientAppService.GetById(clientId);

            return Json(new { client.Address, Phone = client.GetFirstPhone() });
        }

        [HttpPost]
        public ActionResult GetAllWithoutSpecific(int? clientId)
        {
            var client = _clientAppService.Get(x => x.Id != clientId && x.ClientType == ClientType.Client);

            return Json(client);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View("_CreateClient");
        }

        [HttpPost]
        public ActionResult Create(ClientInsertDto dto)
        {
            if (ChechIfPhoneRepeated(dto.ClientPhones))
            {
                _success = false;
                _message = "رقم الهاتف مكرر";
            }
            else
            {
                dto.ClientType = ClientType.Client;
                var clientDto = _clientAppService.Insert(dto);
                if (clientDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء إضافة بيانات العميل";
                }
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var dto = _clientAppService.GetForEdit(id);

            if (Request.IsAjaxRequest())
                return PartialView("_EditClient", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Edit(ClientUpdateDto dto)
        {
            if (ChechIfPhoneRepeated(dto.ClientPhones))
            {
                _success = false;
                _message = "رقم الهاتف مكرر";
            }
            else
            {
                var clientDto = _clientAppService.Update(dto);
                if (clientDto != null)
                    _success = true;
                else
                {
                    _success = false;
                    _message = "حدثت مشكلة اثناء تعديل بيانات العميل";
                }
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var dto = _clientAppService.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView("_DeleteClient", dto);

            return View(dto);
        }

        [HttpPost]
        public ActionResult Delete(ClientDto dto)
        {
            _clientAppService.Delete(dto.Id.Value);

            return Content("success");
        }

        [HttpGet]
        public ActionResult Detail(int id)
        {
            var dto = _clientAppService.GetById(id);

            return View("_DetailsClient", dto);
        }

        #endregion

        public ActionResult UpdateAddressAndPhoneNumberIfNotExist(int clientId, string phone, string address)
        {
            try
            {
                bool isUpdated = false;
                var client = _clientAppService.GetById(clientId);
                if (client != null)
                {
                    if ((client.Address != null && client.Address.Trim() != address.Trim()) || string.IsNullOrEmpty(client.Address))
                    {
                        isUpdated = true;
                        client.Address = address;
                    }

                    if (!client.ClientPhones.Any(x => x.Phone.Trim().Equals(phone.Trim())))
                    {
                        isUpdated = true;
                        var clientPhoneDto = new ClientPhoneDto()
                        {
                            Phone = phone,
                            ClientId = client.Id.Value,
                            IsEnabled = true
                        };
                        client.ClientPhones.Add(clientPhoneDto);

                    }
                    if (isUpdated)
                    {
                        var clientUpdateDto = Mapper.Map<ClientDto, ClientUpdateDto>(client);
                        _clientAppService.Update(clientUpdateDto);
                    }
                }

                _success = true;
            }
            catch (BwrException ex)
            {
                _success = false;
                _message = "حدثت مشكلة اثناء التعديل على معلومات العميل";
            }


            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePhoneNumberIfNotExist(int clientId, string phone)
        {
            try
            {
                var client = _clientAppService.GetById(clientId);
                if (client != null)
                {
                    if (client.ClientPhones.Any(x => x.Phone.Trim().Equals(phone.Trim())))
                    {
                        var clientPhoneDto = new ClientPhoneDto()
                        {
                            Phone = phone,
                            ClientId = client.Id.Value,
                            IsEnabled = true
                        };
                        client.ClientPhones.Add(clientPhoneDto);
                        var clientUpdateDto = Mapper.Map<ClientDto, ClientUpdateDto>(client);
                        _clientAppService.Update(clientUpdateDto);

                    }
                }

                _success = true;
            }
            catch (BwrException ex)
            {
                _success = false;
                _message = "حدثت مشكلة اثناء التعديل على معلومات العميل";
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddNewClientFroTransaction()
        {
            var fullName = Request.Params["fullName"];
            var phone = Request.Params["Phone"];
            var address = Request.Params["address"];
            try
            {
                var checkClientIfExist = _clientAppService.Get(x => x.FullName.Trim().Equals(fullName.Trim())).Any();

                if (!checkClientIfExist)
                {
                    var clientInsertDto = new ClientInsertDto()
                    {
                        FullName = fullName,
                        IsEnabled = true,
                        Address = address,
                        ClientType = ClientType.Normal

                    };

                    var clientDto = _clientAppService.Insert(clientInsertDto);

                    var clientPhoneDto = new ClientPhoneDto()
                    {
                        Phone = phone,
                        IsEnabled = true,
                        ClientId = clientDto.Id.Value
                    };

                    var clientUpdateDto = Mapper.Map<ClientDto, ClientUpdateDto>(clientDto);
                    clientUpdateDto.ClientPhones.Add(clientPhoneDto);
                    _clientAppService.Update(clientUpdateDto);

                    var files = Request.Files;
                    if (files.Count > 0)
                    {
                        int imageType = 0; ;
                        try
                        {
                            imageType = Convert.ToInt32(Request.Params["imageType"]);
                        }
                        catch
                        {
                        }
                        if (imageType != 0 && files != null)
                        {
                            var file = files[0];
                            string extension = @Path.GetExtension(file.FileName);
                            string fileName = ((long)(DateTime.Now - DateTime.MinValue).TotalMilliseconds).ToString() + extension;
                            string path = @Path.Combine(Server.MapPath("~/Images"), fileName);
                            file.SaveAs(path);
                            var clientAttachmentDto = new ClientAttatchmentDto()
                            {
                                AttachmentId = imageType,
                                ClientId = clientDto.Id.Value,
                                Path = "/Images/" + fileName,
                                IsEnabled = true
                            };

                            _clientAttatchmentAppService.Insert(clientAttachmentDto);
                        }
                    }
                    return Json(clientDto.Id);
                }

                _success = true;
            }
            catch (BwrException excption)
            {
                _success = false;
                _message = "حدثت مشكلة اثناء إضافة العميل";
            }

            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateClientImage()
        {
            try
            {
                int clientId = Convert.ToInt32(Request.Params["clientId"]);
                int type = Convert.ToInt32(Request.Params["imageType"]);
                if (type != -1)
                {
                    var file = Request.Files[0];
                    string extension = @Path.GetExtension(file.FileName);
                    string fileName = ((long)(DateTime.Now - DateTime.MinValue).TotalMilliseconds).ToString() + extension;
                    string path = @Path.Combine(Server.MapPath("~/Images"), fileName);
                    file.SaveAs(path);

                    var clientAttachmentDto = new ClientAttatchmentDto()
                    {
                        ClientId = clientId,
                        Path = "/Images/" + fileName,
                        AttachmentId = type
                    };
                    _clientAttatchmentAppService.Insert(clientAttachmentDto);
                }
                _success = true;
            }
            catch
            {
                _success = false;
                _message = "حدثت مشكلة اثناء تعديل الصورة";
            }
            return Json(new { Success = _success, Message = _message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetClientImage(int clientId)
        {
            var clientAttachmentDto = _clientAttatchmentAppService.GetForSpecificClient(clientId).LastOrDefault();

            var client = _clientAppService.GetById(clientId);

            var customar = new
            {
                path = clientAttachmentDto != null ? clientAttachmentDto.Path : string.Empty,
                Id = clientAttachmentDto != null ? clientAttachmentDto.Id : null,
                AttachmentId = clientAttachmentDto != null ? clientAttachmentDto.AttachmentId : 0,
                address = client.Address,
                phone = (client != null && client.ClientPhones.Any()) ? client.ClientPhones.OrderByDescending(x => x.Id).FirstOrDefault() : null
            };
            return Json(customar);
        }

        [HttpPost]
        public void DeleteAddAttachment(int id)
        {
            _clientAttatchmentAppService.Delete(id);
        }

        public ActionResult GetClientInformation(int clientId)
        {
            string address = "";
            string phone = "";
            var client = _clientAppService.GetById(clientId);
            if (client != null)
            {
                address = client.Address;
                phone = client.GetFirstPhone();
            }
            return Json(new { Address = address, Phone = phone });
        }

        [HttpGet]
        public ActionResult GetClientsByName(string fullName)
        {
            try
            {
                var clients = _clientAppService.GetAll().Where(x => x.ClientType == ClientType.Normal);
                return Json(clients.Select(c => new { id = c.Id, text = c.FullName }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AddClientWithJustName(string FullName)
        {
            var clientDto = new ClientInsertDto()
            {
                Address = "",
                FullName = FullName,
                ClientType = ClientType.Normal
            };

            var client = _clientAppService.Insert(clientDto);

            return Json(client.Id);
        }

        #region Helper Method

        private IQueryable<ClientDto> SearchAndSort(IDataTablesRequest requestModel, IQueryable<ClientDto> query)
        {
            // Apply filters
            if (requestModel.Search.Value != string.Empty)
            {
                var value = requestModel.Search.Value.Trim();
                query = query.Where(p => p.FullName.Contains(value)

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

        public bool ChechIfPhoneRepeated(IList<ClientPhoneDto> phones)
        {
            var phonesRepeated = phones.Select(x => x.Phone).Distinct().ToList();

            if (phonesRepeated.Count != phones.Count)
                return true;

            return false;
        }
        [HttpGet]
        public ActionResult GetAgentForSelect2()
        {
            return Json(this._clientAppService.GetSelect2(c=>c.ClientType==ClientType.Client), JsonRequestBehavior.AllowGet);
        }


        #endregion
    }
}