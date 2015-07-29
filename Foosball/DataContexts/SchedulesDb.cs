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
	}
}