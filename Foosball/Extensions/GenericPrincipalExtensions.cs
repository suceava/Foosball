using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace Foosball.Extensions
{
	public static class GenericPrincipalExtensions
	{
		public static string FirstName(this IPrincipal user)
		{
			if (!user.Identity.IsAuthenticated)
			{
				return string.Empty;
			}

			var claims = user.Identity as ClaimsIdentity;
			var claim = claims.FindFirst(c => c.Type == "FirstName");
			if (claim != null)
			{
				return claim.Value;
			}
			else
			{
				return string.Empty;
			}
		}

		public static bool IsAdmin(this IPrincipal user)
		{
			if (!user.Identity.IsAuthenticated)
			{
				return false;
			}

			return user.IsInRole("Admin");
		}
	}
}