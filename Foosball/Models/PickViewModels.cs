using Foosball.DataContexts;
using Foosball.Entities;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Foosball.Models
{
	public class PickViewModel
	{
		public int Id { get; set; }
		public UserListViewModel User { get; set; }
		public ScheduleViewModel Schedule { get; set; }
		public bool? PickHomeTeam { get; set; }
		public int? CombinedScore { get; set; }

		public string GameDateDisplay { get; set; }
		public string GameTimeDisplay { get; set; }

		// indicates if user is allowed to pick this game
		public bool CanPick { get; set; }
		// indicates if it's a pick made by user or a un-picked scheduled game
		public bool IsPick { get; set; }

		#region conversion

		public static PickViewModel FromPick(Pick pick)
		{
			var scheduleModel = ScheduleViewModel.FromSchedule(pick.Schedule);

			return new PickViewModel()
			{
				Id = pick.Id,
				User = (pick.UserId == Pick.MASTER_PICKS_USER_ID ? new UserListViewModel { Id = pick.UserId } : UserListViewModel.FromUser(pick.User)),
				Schedule = scheduleModel,
				PickHomeTeam = pick.PickHomeTeam,
				CombinedScore = pick.CombinedScore,

				GameDateDisplay = scheduleModel.Date.ToString("dddd, MMMM d"),
				GameTimeDisplay = scheduleModel.Date.ToString("t") + " EST",

				CanPick = scheduleModel.IsPickable,
				IsPick = true // coming from a Pick entry => must be a pick
			};
		}

		public Pick ToPick()
		{
			return new Pick
			{
				Id = Id,
				PickHomeTeam = PickHomeTeam.GetValueOrDefault(false),
				CombinedScore = CombinedScore.GetValueOrDefault(0),
				UserId = User.Id,
				ScheduleId = Schedule.Id
			};
		}

		#endregion

		public static PickViewModel GetForSchedule(string userId, int scheduleId)
		{
			return GetFilteredList(userId: userId, scheduleId: scheduleId).FirstOrDefault();
		}

		public static List<PickViewModel> GetListForUser(string userId, int? week)
		{
			return GetFilteredList(userId: userId, week: week);
		}

		public static List<PickViewModel> GetListForWeek(int week)
		{
			return GetFilteredList(week: week);
		}

		private static List<PickViewModel> GetFilteredList(int? id = null, string userId = null, int? week = null, int? scheduleId = null)
		{
			var predicate = PredicateBuilder.True<Pick>();
			if (id.HasValue)
			{
				predicate = predicate.And(p => p.Id == id.Value);
			}
			if (week.HasValue)
			{
				predicate = predicate.And(p => p.Schedule.Week == week.Value);
			}
			if (userId != null)
			{
				predicate = predicate.And(p => p.UserId == userId);
			}
			if (scheduleId.HasValue)
			{
				predicate = predicate.And(p => p.Schedule.Id == scheduleId.Value);
			}

			using (var db = new PicksDb())
			{
				return db.Picks
					.Include("Schedule")
					.OrderBy(p => p.Schedule.Date)
					.ThenBy(p => p.ScheduleId)
					.AsExpandable()
					.Where(predicate)
					.ToList()
					.Select(p => PickViewModel.FromPick(p))
					.ToList();
			}
		}

		public void Save()
		{
			var pick = ToPick();

			using (var db = new PicksDb())
			{
				db.Entry(pick).State = (Id == 0) ? EntityState.Added : EntityState.Modified;

				db.SaveChanges();
			}
		}
	}

	public class AllPicksViewModel
	{
		public UserListViewModel User { get; set; }
		public Dictionary<int, TeamViewModel> PickedTeams { get; }
		public int? CombinedScore { get; set; }

		public AllPicksViewModel()
		{
			PickedTeams = new Dictionary<int, TeamViewModel>();
		}

		public static List<AllPicksViewModel> GetListForWeek(int week)
		{
			// get schedules for the week
			var schedules = ScheduleViewModel.GetList(week);

			// get all the picks for the week
			var allPicks = PickViewModel.GetListForWeek(week);

			// get the master picks
			var masterPicks = ForUserFromPicks(allPicks, new UserListViewModel { Id = Pick.MASTER_PICKS_USER_ID }, schedules);

			var listAllPicks = new List<AllPicksViewModel>();
			// first element in list is master picks
			listAllPicks.Add(masterPicks);

			// get all users
			var users = UserListViewModel.GetList();
			// add all non-guest users
			foreach (var user in users.Where(u => u.Role != "Guest").OrderBy(u => u.FirstName + " " + u.LastName))
			{
				listAllPicks.Add(ForUserFromPicks(allPicks, user, schedules));
			}

			return listAllPicks;
		}

		private static AllPicksViewModel ForUserFromPicks(List<PickViewModel> picks, UserListViewModel user, List<ScheduleViewModel> schedules)
		{
			var model = new AllPicksViewModel
			{
				User = user
			};

			// add all schedules with null pick
			foreach (var schedule in schedules.OrderBy(s => s.Date).ThenBy(s => s.Id))
			{
				model.PickedTeams.Add(schedule.Id, null);
			}

			if (picks != null)
			{
				// only add the teams for locked games
				foreach (var pick in picks.Where(p => p.User.Id == user.Id && !p.CanPick))
				{
					model.PickedTeams[pick.Schedule.Id] = pick.PickHomeTeam.GetValueOrDefault(false) ? pick.Schedule.HomeTeam : pick.Schedule.AwayTeam;
					if (pick.Schedule.RequireScore)
					{
						model.CombinedScore = pick.CombinedScore.GetValueOrDefault(0);
					}
				}
			}

			return model;
		}

  	}
}