using Foosball.DataContexts;
using Foosball.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Foosball.Controllers
{
	[Authorize(Roles = "Admin,User")]
	public class PicksController : BaseController
    {
        [HttpGet]
        public ActionResult Index(int? week)
        {
			ViewBag.CurrentWeek = week ?? SchedulesDb.GetCurrentWeek();
            return View();
        }

		[HttpGet]
		public ActionResult ListData(int? week)
		{
			var userId = User.Identity.GetUserId();
			var user = new UserListViewModel();
			var currentWeek = week ?? SchedulesDb.GetCurrentWeek();

			// picks already mad
			var picks = PickViewModel.GetListForUser(userId, currentWeek);
			// full schedule for this week
			var schedules = ScheduleViewModel.GetList(currentWeek);

			// add all missing schedules to the picks list
			foreach (var schedule in schedules.Where(s => !picks.Exists(p => p.Schedule.Id == s.Id)))
			{
				picks.Add(new PickViewModel
				{
					CanPick = schedule.IsPickable,
					IsPick = false,

					GameDateDisplay = schedule.Date.ToString("dddd, MMMM d"),
					GameTimeDisplay = schedule.Date.ToString("t") + " EST",

					Schedule = schedule,
					User = user
				});
			}

			var tieBreaker = picks.Find(p => p.Schedule.RequireScore);
			if (tieBreaker != null)
			{
				tieBreaker.GameDateDisplay = "TIE BREAKER - " + tieBreaker.GameDateDisplay;
			}

            return Json(new { data = picks.OrderBy(p => p.Schedule.RequireScore).ThenBy(p => p.Schedule.Date).ToList() }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult Pick(PickViewModel model)
		{
			var userId = User.Identity.GetUserId();

			#region validation

			if (model == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid pick");
			}
			if (model.Schedule == null || model.Schedule.Id <= 0)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid schedule Id");
			}

			var schedule = ScheduleViewModel.Get(model.Schedule.Id);
			if (schedule == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Schedule not found");
			}
			if (!schedule.IsPickable)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Game is not pickable");
			}
			if (!schedule.RequireScore)
			{
				// if game doesn't require a score, zero out the score
				model.CombinedScore = 0;
			}

			#endregion

			// load pick if already exists
			var pick = PickViewModel.GetForSchedule(userId, model.Schedule.Id);
			if (pick == null)
			{
				pick = new PickViewModel
				{
					User = new UserListViewModel { Id = userId },
					Schedule = new ScheduleViewModel { Id = model.Schedule.Id }
				};
			}

			if (model.PickHomeTeam.HasValue)
			{
				pick.PickHomeTeam = model.PickHomeTeam;
			}
			if (model.CombinedScore.HasValue)
			{
				pick.CombinedScore = model.CombinedScore;
			}

			pick.Save();

			return Json(pick.Id);
		}
	}
}