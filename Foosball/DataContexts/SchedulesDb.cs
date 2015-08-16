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
				return db.Schedules.Where(s => s.Date >= DateTime.Now).Min(s => (int?)s.Week).GetValueOrDefault(1);
            }
		}

		public static int GetWeekCount()
		{
			using (var db = new SchedulesDb())
			{
				return db.Schedules.Select(s => s.Week).Distinct().Count();
			}
		}
	}
}