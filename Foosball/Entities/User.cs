using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Foosball.Entities
{
	// You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
	[Table("AspNetUsers")]
	public class User : IdentityUser
	{
		[StringLength(100)]
		public string FirstName { get; set; }
		[StringLength(100)]
		public string LastName { get; set; }
		[StringLength(1000)]
		public string ImageUrl { get; set; }

		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
		{
			// Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
			var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
			// Add custom user claims here
			userIdentity.AddClaim(new Claim("FirstName", FirstName));
			userIdentity.AddClaim(new Claim("LastName", LastName));
			userIdentity.AddClaim(new Claim("ImageUrl", ImageUrl ?? ""));

			return userIdentity;
		}
	}
}