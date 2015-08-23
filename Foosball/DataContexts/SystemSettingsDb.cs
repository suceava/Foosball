using Foosball.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Foosball.DataContexts
{
	public class SystemSettingsDb : DbContext
	{
		public SystemSettingsDb() : base("DefaultConnection") { }

		public DbSet<SystemSettings> SystemSettings { get; set; }
	}
}