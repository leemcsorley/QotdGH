using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QotdMvc.Controllers
{
    public class AlertController : Controller
    {
        //
        // GET: /Alert/

        public ActionResult AnswerSuccess()
        {
            return View();
        }

        public ActionResult QuestionSuccess()
        {
            return View();
        }
    }
}
