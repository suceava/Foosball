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
		public ActionResult Index(int? week, bool? isMaster)
		{
			ViewBag.CurrentWeek = SchedulesDb.GetCurrentWeek();
			ViewBag.Week = week.GetValueOrDefault(SchedulesDb.GetCurrentWeek());
			ViewBag.IsMaster = isMaster.GetValueOrDefault(false);

			return View();
		}

		[HttpGet]
		public ActionResult ListData(int? week, bool? isMaster)
		{
			#region validation

			if (!User.IsInRole("Admin"))
			{
				isMaster = null;
			}

			#endregion

			var masterUserId = Guid.Empty.ToString();
            var userId = (isMaster.GetValueOrDefault(false) ? masterUserId : User.Identity.GetUserId());
			var user = new UserListViewModel();
			var currentWeek = SchedulesDb.GetCurrentWeek();
			var picksWeek = week.GetValueOrDefault(currentWeek);

			// picks already made
			var picks = PickViewModel.GetListForUser(userId, picksWeek);
			if (!isMaster.GetValueOrDefault(false) && picksWeek < currentWeek)
			{
				// get master picks to see if picks were correct
				var masterPicks = PickViewModel.GetListForUser(masterUserId, picksWeek);
				foreach (var p in picks)
				{
					var masterPick = masterPicks.Find(m => m.Schedule.Id == p.Schedule.Id);
                    p.IsCorrect = (masterPick != null && masterPick.PickHomeTeam == p.PickHomeTeam);
				}
			}

			// full schedule for this week
			var schedules = ScheduleViewModel.GetList(picksWeek);

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
				tieBreaker.GameDateDisplay = " " + tieBreaker.GameDateDisplay;
			}

			if (isMaster.GetValueOrDefault(false))
			{
				// for master pick, always allow picking
				picks.ForEach(p => p.CanPick = true);
			}
			else if (picksWeek > currentWeek)
			{
				// if looking at future picks, don't allow picking
				picks.ForEach(p => p.CanPick = false);
			}

			return Json(new { data = picks.OrderBy(p => p.Schedule.RequireScore).ThenBy(p => p.Schedule.Date).ThenBy(p => p.Schedule.Id).ToList() }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult Pick(PickViewModel model, bool? isMaster)
		{
			#region validation

			if (!User.IsInRole("Admin"))
			{
				isMaster = null;
			}

			var userId = (isMaster.GetValueOrDefault(false) ? Guid.Empty.ToString() : User.Identity.GetUserId());

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
			if (!schedule.IsPickable && !isMaster.GetValueOrDefault(false) || schedule.Week > SchedulesDb.GetCurrentWeek())
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

		[HttpGet]
		public ActionResult All(int? week)
		{
			var currentWeek = SchedulesDb.GetCurrentWeek();
			var picksWeek = week.GetValueOrDefault(currentWeek);

			ViewBag.CurrentWeek = currentWeek;
			ViewBag.Week = picksWeek;

			bool hasLockedSchedule;
			var allPicks = AllPicksViewModel.GetListForWeek(picksWeek, out hasLockedSchedule);
			ViewBag.NoGamesLocked = !hasLockedSchedule;

			return View(allPicks);
		}

		[HttpGet]
		public ActionResult Standings()
		{
			var standings = StandingsViewModel.GetList();

			return View(standings);
		}

		[HttpGet]
		public ActionResult Top20()
		{
			var standings = StandingsViewModel.GetList();

			return PartialView(standings.Take(15).ToList());
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public ActionResult NoPicks()
		{
			var currentWeek = SchedulesDb.GetCurrentWeek();
			if (currentWeek == 0)
			{
				return null;
			}
			var users = UserListViewModel.GetListWithNoPicks(currentWeek);

			ViewBag.CurrentWeek = currentWeek;

            return View(users);
		}
	}
}