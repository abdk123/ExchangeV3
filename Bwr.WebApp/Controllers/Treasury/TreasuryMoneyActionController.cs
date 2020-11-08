﻿using Bwr.WebApp.Models.Security;
using BWR.Application.Common;
using BWR.Application.Dtos.Treasury.TreasuryMoneyAction;
using BWR.Application.Interfaces.Shared;
using BWR.Application.Interfaces.TreasuryMoneyAction;
using System.Linq;
using System.Web.Mvc;

namespace Bwr.WebApp.Controllers.Treasury
{
    public class TreasuryMoneyActionController : Controller
    {
        private readonly ITreasuryMoneyActionAppService _treasuryMoneyActionAppService;
        private readonly IAppSession _appSession;
        private string _message;
        private bool _success;

        public TreasuryMoneyActionController(ITreasuryMoneyActionAppService treasuryMoneyActionAppService, IAppSession appSession)
        {
            _treasuryMoneyActionAppService = treasuryMoneyActionAppService;
            _message = "";
            _success = false;
            _appSession = appSession;
        }

        public ActionResult Detail(int? id)
        {
            if (id != null)
            {
                return RedirectToAction("BoxActionDetails", "BoxAction", new { moneyActionId = id });
            }

            return null;
        }

        public ActionResult Index()
        {
            var entityDto = new EntityDto()
            {
                Id = new AppSession().GetCurrentTreasuryId()
            };
            return View(entityDto);
        }

        public ActionResult TreasuryBalance(int treasuryId)
        {
            TempData["TreasuryId"] = treasuryId.ToString();

            var treasuryMoneyActionInsertDto = new TreasuryMoneyActionInsertDto()
            {
                TreasuryId = treasuryId
            };
            return View(treasuryMoneyActionInsertDto);
        }

        public ActionResult TreasuryActions(int treasuryId)
        {
            var entityDto = new EntityDto()
            {
                Id = treasuryId
            };
            return View(entityDto);
        }

        // GET: TreasuryMoneyActionFlow
        public ActionResult Get(TreasuryMoneyActionInputDto input)
        {
            var treasuryMoneyActiones = _treasuryMoneyActionAppService.Get(input).OrderBy(x=>x.Id);
            return Json(new { data = treasuryMoneyActiones }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTreasuryActions(TreasuryMoneyActionInputDto input)
        {
            var treasuryActiones = _treasuryMoneyActionAppService.GetMoneyActions(input).OrderBy(x => x.Date);
            return Json(new { data = treasuryActiones }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(TreasuryMoneyActionInsertDto dto)
        {
            var treasuryId = 0;
            TreasuryMoneyActionDto treasuryMoneyActionDto;
            if (dto.ActionType == 1)
            {
                treasuryMoneyActionDto = _treasuryMoneyActionAppService.GetMony(dto);
            }
            else
            {
                treasuryMoneyActionDto = _treasuryMoneyActionAppService.GiveMony(dto);
            }
            if (treasuryMoneyActionDto != null)
            {
                _success = true;
                treasuryId = treasuryMoneyActionDto.TreasuryId;
            }
            else
            {
                _success = false;
                _message = "حدثت مشكلة اثناء إضافة البيانات";
            }

            return Json(new { Success = _success, Message = _message,TreasuryId= treasuryId }, JsonRequestBehavior.AllowGet);
        }

    }
}