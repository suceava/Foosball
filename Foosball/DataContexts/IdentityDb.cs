using Foosball.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Foosball.DataContexts
{
	public class IdentityDb : IdentityDbContext<User>
	{
		public IdentityDb()
			: base("DefaultConnection", throwIfV1Schema: false)
		{
		}

		public static IdentityDb Create()
		{
			return new IdentityDb();
		}
	}
}