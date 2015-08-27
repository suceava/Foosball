using Foosball.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Foosball.DataContexts
{
	public class SchedulesDb : DbContext
	{
		public SchedulesDb() : base("DefaultConnection") { }

		public DbSet<Schedule> Schedules { get; set; }

		public static int GetCurrentWeek()
		{
			using (var db = new SchedulesDb())
			{
				var maxWeek = db.Schedules.Max(s => s.Week);
				return db.Schedules.Where(s => s.Date >= DateTime.Now).Min(s => (int?)s.Week).GetValueOrDefault(maxWeek);
            }
		}

		public static int GetWeekCount()
		{
			using (var db = new SchedulesDb())
			{
				return db.Schedules.Select(s => s.Week).Distinct().Count();
			}
		}

		public static bool IsWeekLocked(int week)
		{
			using (var db = new SchedulesDb())
			{
				return db.Schedules.Where(s => s.Week == week).All(s => !s.IsPickable);
			}
		}
	}
}