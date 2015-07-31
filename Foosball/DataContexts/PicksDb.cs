using Foosball.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Foosball.DataContexts
{
	public class PicksDb : DbContext
	{
		public PicksDb() : base("DefaultConnection") { }

		public DbSet<Pick> Picks { get; set; }
	}
}