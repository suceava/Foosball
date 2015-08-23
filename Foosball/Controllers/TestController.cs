using Foosball.Entities;
using Foosball.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Foosball.Controllers
{
	[Authorize(Roles = "Admin")]
    public class TestController : BaseController
    {
		[HttpPost]
		public async Task<ActionResult> GenerateTestData()
		{
#if DEBUG
			var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

			var users = new List<User>();

			// add users
			for (var i = 1; i <= 75; i++)
			{
				var email = string.Format("test.user.{0}@mail.com", i);
				var user = new User
				{
					UserName = email,
					Email = email,
					FirstName = "Test" + i,
					LastName = "User"
				};
				var result = await userManager.CreateAsync(user, "Password01!");
				if (result.Succeeded)
				{
					var currentUser = userManager.FindByEmail(user.Email);

					var roleresult = userManager.AddToRole(currentUser.Id, "User");

					users.Add(currentUser);
				}
			}

			for (var week = 1; week <= 3; week++)
			{
				// get week  schedules
				var schedules = ScheduleViewModel.GetList(week);
				var rnd = new Random();

				// add the picks for each user for week 1
				foreach (var user in users)
				{
					foreach (var schedule in schedules)
					{
						var pick = new PickViewModel
						{
							User = UserListViewModel.FromUser(user),
							Schedule = schedule,
							PickHomeTeam = (rnd.Next(0, 100) > 50),
							CombinedScore = (schedule.RequireScore ? rnd.Next(3, 50) : 0)
						};
						pick.Save();
					}
				}
			}
#endif

			return null;
		}
    }
}