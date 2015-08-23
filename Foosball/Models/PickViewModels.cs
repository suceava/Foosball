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

		public static void DeleteForUser(string userId)
		{
			var picks = PickViewModel.GetListForUser(userId, null);
			using (var db = new PicksDb())
			{
				foreach (var pick in picks)
				{
					var p = pick.ToPick();

					db.Entry(p).State = EntityState.Deleted;
				}
				db.SaveChanges();
			}
		}
	}

	public class AllPicksViewModel
	{
		public UserListViewModel User { get; set; }
		public Dictionary<int, TeamViewModel> PickedTeams { get; }
		public int? CombinedScore { get; set; }
		public int CorrectPicks { get; set; }

		public AllPicksViewModel()
		{
			PickedTeams = new Dictionary<int, TeamViewModel>();
		}

		public static List<AllPicksViewModel> GetListForWeek(int week, out bool hasLockedSchedule)
		{
			// get schedules for the week
			var schedules = ScheduleViewModel.GetList(week);
			hasLockedSchedule = schedules.Any(s => !s.IsPickable);

			// get all the picks for the week
			var allPicks = PickViewModel.GetListForWeek(week);

			// get the master picks
			var masterPicks = ForUserFromPicks(allPicks, new UserListViewModel { Id = Pick.MASTER_PICKS_USER_ID }, schedules, null);

			var listAllPicks = new List<AllPicksViewModel>();

			// get all users
			var users = UserListViewModel.GetList();
			// add all non-guest users
			foreach (var user in users.Where(u => u.Role != "Guest"))
			{
				listAllPicks.Add(ForUserFromPicks(allPicks, user, schedules, masterPicks));
			}

			listAllPicks = listAllPicks.OrderByDescending(p => p.CorrectPicks).ThenBy(p => p.User.FirstName + " " + p.User.LastName).ToList();

			// first element in list is master picks
			listAllPicks.Insert(0, masterPicks);

			return listAllPicks;
		}

		private static AllPicksViewModel ForUserFromPicks(List<PickViewModel> picks, UserListViewModel user, List<ScheduleViewModel> schedules, AllPicksViewModel masterPicks)
		{
			var model = new AllPicksViewModel
			{
				User = user,
				CorrectPicks = 0
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
					var userPick = pick.PickHomeTeam.GetValueOrDefault(false) ? pick.Schedule.HomeTeam : pick.Schedule.AwayTeam;
                    model.PickedTeams[pick.Schedule.Id] = userPick;
					if (pick.Schedule.RequireScore)
					{
						model.CombinedScore = pick.CombinedScore.GetValueOrDefault(0);
					}

					if (masterPicks != null)
					{
						// find the master pick for this schedule
						var masterPick = masterPicks.PickedTeams.ContainsKey(pick.Schedule.Id) ? masterPicks.PickedTeams[pick.Schedule.Id] : null;
						if (masterPick != null && masterPick.Id == userPick.Id)
						{
							// match
							model.CorrectPicks++;
						}
					}
				}
			}

			return model;
		}
  	}

	public class StandingsViewModel
	{
		public UserListViewModel User { get; set; }
		public List<int> WeeklyPoints { get; }
		public int Place { get; set; }

		public StandingsViewModel()
		{
			WeeklyPoints = new List<int>();
		}

		public static List<StandingsViewModel> GetList()
		{
			var maxWeek = SchedulesDb.GetCurrentWeek();
			var listStandings = new List<StandingsViewModel>();
			bool hasLockedSchedule;

			for (var week = 1; week <= maxWeek; week++)
			{
				// get all picks for each week
				var allPicks = AllPicksViewModel.GetListForWeek(week, out hasLockedSchedule);
				foreach (var pick in allPicks.Where(p => p.User.Id != Pick.MASTER_PICKS_USER_ID))
				{
					var standing = listStandings.Find(s => s.User.Id == pick.User.Id);
					if (standing == null)
					{
						standing = new StandingsViewModel
						{
							User = pick.User
						};
						listStandings.Add(standing);
					}
					standing.WeeklyPoints.Add(pick.CorrectPicks);
				}
			}

			// sort by total points
			listStandings = listStandings.OrderByDescending(s => s.WeeklyPoints.Sum()).ToList();

			// set the Place
			for (var i=0; i<listStandings.Count; i++)
			{
				var standing = listStandings[i];
				standing.Place = i + 1;
			}

			return listStandings;
		}
	}
}