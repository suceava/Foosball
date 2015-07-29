using Foosball.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Foosball.DataContexts
{
	public class TeamsDb : DbContext
	{
		public TeamsDb() : base("DefaultConnection") {}

		public DbSet<Team> Teams { get; set; }
	}
}