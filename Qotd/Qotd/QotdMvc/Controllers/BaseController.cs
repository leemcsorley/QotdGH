﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Qotd.Data;
using Qotd.Entities;
using QotdMvc.Service;
using Qotd.PresentationObjects;
using System.Transactions;

namespace QotdMvc.Controllers
{
    public abstract class BaseController : Controller
    {
        private IDataProvider _dataProvider;
        private IQotdService _qotdService;
        private UserPO _userPO;
        private Question _todaysQuestion;

        private static readonly Dictionary<string, Func<dynamic, string>> FriendlyPageNames = new Dictionary<string, Func<dynamic, string>>()
        {
            { "Home-Index", d => "Home" },
            { "Home-User", d => d.TargetUser.User.DisplayName },
            { "Home-Tag", d => d.Tag.Tag.Value },
            { "Home-Question", d => "View Question" },
            { "Home-Answer", d => "View Answer" },
            { "Home-AllNotifications", d => "Notifications" }
        };

        protected TransactionScope CreateTransactionScope()
        {
            return new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions()
                {
                    IsolationLevel = IsolationLevel.ReadUncommitted,
                    Timeout = new TimeSpan(0, 15, 0)
                });
        }

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

        protected UserPO UserPO
        {
            get
            {
                if (_userPO == null)
                {
                    if (User != null && User.Identity.IsAuthenticated)
                    {
                        _userPO = DataProvider.GetUserByUsername(User.Identity.Name);
                    }
                }
                return _userPO;
            }
        }

        protected User UserEntity
        {
            get
            {
                if (UserPO == null) return null;
                return UserPO.User;
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

            using (TransactionScope scope = CreateTransactionScope())
            {
                ViewBag.IsAuthenticated = User != null && User.Identity.IsAuthenticated;
                ViewBag.User = UserEntity;
                ViewBag.Question = TodaysQuestion;
                ViewBag.UserPO = UserPO;
                string controllerName = (string)this.RouteData.Values["Controller"];
                string actionName = (string)this.RouteData.Values["Action"];
                ViewBag.PageController = controllerName;
                ViewBag.PageAction = actionName;
                string fn = GetPageFriendlyName();
                ViewBag.PageFriendlyName = GetPageFriendlyName();
                ViewBag.PageUrl = GetPageUrl();
                scope.Complete();
            }
        }

        protected string GetPageUrl()
        {
            return HttpContext.Request.RawUrl;
        }

        protected string GetPageFriendlyName()
        {
            string n = this.RouteData.Values["Controller"] + "-" + this.RouteData.Values["Action"];
            if (FriendlyPageNames.ContainsKey(n))
                return FriendlyPageNames[n](ViewBag);
            if (ViewBag.PageFriendlyName == null)
                return "";
            return "";
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
