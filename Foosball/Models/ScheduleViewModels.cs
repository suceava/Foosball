using Foosball.DataContexts;
using Foosball.Entities;
using LinqKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace Foosball.Models
{
	public class ScheduleViewModel
	{
		public int Id { get; set; }
		public int Week { get; set; }
		[Display(Name = "Date & Time (in EST)")]
		[JsonConverter(typeof(IsoDateTimeConverter))]
		public DateTime Date { get; set; }
		[Display(Name = "Home Team")]
		public TeamViewModel HomeTeam { get; set; }
		[Display(Name = "Away Team")]
		public TeamViewModel AwayTeam { get; set; }
		[Display(Name = "Require Score")]
		public bool RequireScore { get; set; }
		[Display(Name = "Is Pickable")]
		public bool IsPickable { get; set; }

		public string DateDisplay { get; set; }

		public ScheduleViewModel()
		{
			IsPickable = true;
		}

		#region conversion

		public static ScheduleViewModel FromSchedule(Schedule schedule)
		{
			return new ScheduleViewModel()
			{
				Id = schedule.Id,
				Week = schedule.Week,
				Date = schedule.Date,
				HomeTeam = TeamViewModel.FromTeam(schedule.HomeTeam),
				AwayTeam = TeamViewModel.FromTeam(schedule.AwayTeam),
				RequireScore = schedule.RequireScore,
				IsPickable = schedule.IsPickable,

				DateDisplay = schedule.Date.ToString("dddd, MMMM d") + " at " + schedule.Date.ToString("h:mm tt") + " EST"
			};
        }

		public Schedule ToSchedule()
		{
			return new Schedule()
			{
				Id = Id,
				Week = Week,
				Date = Date,
				HomeTeamId = HomeTeam.Id,
				AwayTeamId = AwayTeam.Id,
				RequireScore = RequireScore,
				IsPickable = IsPickable
			};
		}

		#endregion

		public static ScheduleViewModel Get(int id)
		{
			if (id == 0)
			{
				return new ScheduleViewModel()
				{
					Date = DateTime.Today
				};
			}

			return GetFilteredList(id: id).FirstOrDefault();
		}

		public static List<ScheduleViewModel> GetList(int? week = null)
		{
			return GetFilteredList(week: week);
		}

		private static List<ScheduleViewModel> GetFilteredList(int? id = null, int? week = null)
		{ 
			var predicate = PredicateBuilder.True<Schedule>();
			if (id.HasValue)
			{
				predicate = predicate.And(s => s.Id == id.Value);
			}
			if (week.HasValue)
			{
				predicate = predicate.And(s => s.Week == week.Value);
			}

			using (var db = new SchedulesDb())
			{
				return db.Schedules.Include("HomeTeam").Include("AwayTeam").OrderBy(s => s.Week).AsExpandable().Where(predicate).ToList().Select(s => ScheduleViewModel.FromSchedule(s)).ToList();
			}
		}

		public void Save()
		{
			var schedule = ToSchedule();
            using (var db = new SchedulesDb())
			{
				db.Entry(schedule).State = (Id == 0) ? EntityState.Added : EntityState.Modified;

				db.SaveChanges();
			}
		}

		public void Delete()
		{
			var schedule = ToSchedule();

            using (var db = new SchedulesDb())
			{
				db.Entry(schedule).State = EntityState.Deleted;

				db.SaveChanges();
			}
		}
	}
}