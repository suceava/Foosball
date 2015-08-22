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

			return View();
		}
	}
}
