using Foosball.DataContexts;
using Foosball.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Foosball.Controllers
{
	[Authorize]
	public class HomeController : BaseController
	{
		public ActionResult Index()
		{
			ViewBag.Winnings = CurrentUser != null ? CurrentUser.Winnings : 0;
			ViewBag.FullName = CurrentUser.FirstName + " " + CurrentUser.LastName;
			ViewBag.ImageUrl = CurrentUser.ImageUrl;
			ViewBag.Announcement = AnnouncementViewModel.Get().Announcement;

			// get picks and schedules to figure out missing picks
			var userId = User.Identity.GetUserId();
			var currentWeek = SchedulesDb.GetCurrentWeek();

			ViewBag.MissingPicks = PickViewModel.MissingPicksForUser(userId, currentWeek);

			return View();
		}
	}
}
