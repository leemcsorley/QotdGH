﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Qotd.Data;
using Qotd.Entities;
using QotdMvc.Service;

namespace QotdMvc.Controllers
{
    public abstract class BaseController : Controller
    {
        private IDataProvider _dataProvider;
        private IQotdService _qotdService;
        private User _userEntity;
        private Question _todaysQuestion;

        protected IDataProvider DataProvider
        {
            get
            {
                if (_dataProvider == null)
                {
                    _dataProvider = new QotdContext();
                }
                return _dataProvider;
            }
        }

        protected IQotdService QotdService
        {
            get
            {
                if (_qotdService == null)
                {
                    _qotdService = new QotdService(DataProvider);
                }
                return _qotdService;
            }
        }

        protected User UserEntity
        {
            get
            {
                if (_userEntity == null)
                {
                    if (User != null && User.Identity.IsAuthenticated)
                    {
                        _userEntity = DataProvider.GetUserByUsername(User.Identity.Name);
                    }
                }
                return _userEntity;
            }
        }

        protected Question TodaysQuestion
        {
            get
            {
                if (_todaysQuestion == null)
                {
                    _todaysQuestion = DataProvider.GetTodaysQuestion();
                }
                return _todaysQuestion;
            }
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            ViewBag.IsAuthenticated = User != null && User.Identity.IsAuthenticated;
            ViewBag.User = UserEntity;
            ViewBag.Question = TodaysQuestion;
        }

        protected virtual ActionResult JsonSuccess()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        protected virtual ActionResult JsonError(string error)
        {
            return Json(new { error = error }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_dataProvider != null) 
                    _dataProvider.Dispose();
            }
        }
    }
}
