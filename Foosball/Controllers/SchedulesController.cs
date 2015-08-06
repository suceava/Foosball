using Foosball.DataContexts;
using Foosball.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Foosball.Controllers
{
    public class SchedulesController : BaseController
    {
		[HttpGet]
		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public ActionResult ListData()
		{
			return Json(new { data = ScheduleViewModel.GetList() }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult Edit(int id)
		{
			ViewData.Add("Teams", TeamViewModel.GetList());
			return View(ScheduleViewModel.Get(id));
		}

		[HttpPost]
		public ActionResult Edit(ScheduleViewModel schedule)
		{
			#region validation

			if (schedule == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid schedule");
			}
			if (schedule.HomeTeam == null || schedule.AwayTeam == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid schedule teams");
			}
			if (schedule.HomeTeam.Id == schedule.AwayTeam.Id)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Home and away teams cannot be the same");
			}

			// load the teams from the Db
			schedule.AwayTeam = TeamViewModel.Get(schedule.AwayTeam.Id);
			schedule.HomeTeam = TeamViewModel.Get(schedule.HomeTeam.Id);

			// re-validate teams
			if (schedule.HomeTeam == null || schedule.AwayTeam == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid schedule teams");
			}

			#endregion

			schedule.Save();

			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult Delete(int id)
		{
			var schedule = ScheduleViewModel.Get(id);
			if (schedule == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.NotFound);
			}

			schedule.Delete();

            return Json(null);
		}
	}
}