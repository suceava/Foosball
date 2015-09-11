using Foosball.DataContexts;
using Foosball.Entities;
using LinqKit;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace Foosball.Models
{
	public class UserListViewModel
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string ImageUrl { get; set; }
		public string Role { get; set; }
		public double Winnings { get; set; }

		#region conversion

		public static UserListViewModel FromUser(User user, IDbSet<IdentityRole> roles = null)
		{
			var role = string.Empty;
			if (roles != null)
			{
				// find the role names
				foreach (var userRole in user.Roles)
				{
					if (role.Length > 0)
						role += ",";
					role += roles.First(r => r.Id == userRole.RoleId).Name;
				}
			}

			return new UserListViewModel()
			{
				Id = user.Id,
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				ImageUrl = user.ImageUrl,
				Role = role,
				Winnings = user.Winnings
			};
		}

		#endregion

		public static List<UserListViewModel> GetList()
		{
			return GetFilteredList(id: null);
		}

		private static List<UserListViewModel> GetFilteredList(string id = null)
		{
			var predicate = PredicateBuilder.True<User>();
			if (id != null)
			{
				predicate = predicate.And(u => u.Id == id);
			}

			using (var db = IdentityDb.Create())
			{
				return db.Users.OrderBy(u => u.Email).Where(predicate).ToList().Select(u => UserListViewModel.FromUser(u, db.Roles)).ToList();
			}
		}

		public static List<UserListViewModel> GetListWithNoPicks(int week)
		{
			bool b;
			var allPicks = AllPicksViewModel.GetListForWeek(week, out b);

			return allPicks
				.Where(p => p.PickedTeams.Values.All(t => t == null))
				.Select(p => p.User)
				.ToList();
		}
	}

	public class ForgotViewModel
	{
		[Required]
		[Display(Name = "Email")]
		public string Email { get; set; }
	}

	public class LoginViewModel
	{
		[Required]
		[Display(Name = "Email")]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}

	public class UserEditViewModel
	{
		public string Id { get; set; }

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Display(Name = "First")]
		[StringLength(100)]
		public string FirstName { get; set; }

		[Display(Name = "Last")]
		[StringLength(100)]
		public string LastName { get; set; }
	}

	public class RegisterViewModel
	{
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Display(Name = "First")]
		[StringLength(100)]
		public string FirstName { get; set; }

		[Display(Name = "Last")]
		[StringLength(100)]
		public string LastName { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}

	public class ResetPasswordViewModel
	{
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		public string Code { get; set; }
	}

	public class ForgotPasswordViewModel
	{
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }
	}
}
