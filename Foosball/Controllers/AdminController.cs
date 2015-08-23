using Foosball.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Foosball.Controllers
{
	[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
		[HttpGet]
		public ActionResult Announcement()
		{
			return View(AnnouncementViewModel.Get());
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Announcement(string announcement)
		{
			var vm = AnnouncementViewModel.Get();
			vm.Announcement = announcement;
			vm.Save();

			return RedirectToAction("Index", "Home");
		}
    }
}