using Foosball.DataContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Foosball.Controllers
{
    public class TeamsController : BaseController
    {
		[HttpGet]
		public ActionResult Index()
        {
            return View();
        }

		[HttpGet]
		public ActionResult ListData()
		{
			var list = new TeamsDb().Teams.ToList();

			return Json(new { data = list }, JsonRequestBehavior.AllowGet);
		}
	}
}