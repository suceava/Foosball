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

		// indicates if user is allowed to pick this game
		public bool CanPick { get; set; }
		// indicates if it's a pick made by user or a un-picked scheduled game
		public bool IsPick { get; set; }
		// indicates if UI should show score
		public bool ShowCombinedScore
		{
			get
			{
				return Schedule == null ? false : Schedule.IsMondayGame();
			}
		}

		#region conversion

		public static PickViewModel FromPick(Pick pick)
		{
			var scheduleModel = ScheduleViewModel.FromSchedule(pick.Schedule);

			return new PickViewModel()
			{
				Id = pick.Id,
				User = UserListViewModel.FromUser(pick.User),
				Schedule = scheduleModel,
				PickHomeTeam = pick.PickHomeTeam,
				CombinedScore = pick.CombinedScore,

				CanPick = scheduleModel.IsPickable(),
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
				return db.Picks.Include("Schedule").OrderBy(p => p.Schedule.Date).AsExpandable().Where(predicate).ToList().Select(p => PickViewModel.FromPick(p)).ToList();
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
}