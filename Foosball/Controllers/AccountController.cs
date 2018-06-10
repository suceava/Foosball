using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Foosball.Entities;
using Foosball.Models;
using System.Text;
using System.Net;
using System.Configuration;

namespace Foosball.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;

		public AccountController()
		{
		}

		public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
		{
			UserManager = userManager;
			SignInManager = signInManager;
		}

		public ApplicationSignInManager SignInManager
		{
			get
			{
				return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
			}
			private set
			{
				_signInManager = value;
			}
		}

		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set
			{
				_userManager = value;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_userManager != null)
				{
					_userManager.Dispose();
					_userManager = null;
				}

				if (_signInManager != null)
				{
					_signInManager.Dispose();
					_signInManager = null;
				}
			}

			base.Dispose(disposing);
		}

		#region ASP.NET Identity

		// The Authorize Action is the end point which gets called when you access any
		// protected Web API. If the user is not logged in then they will be redirected to 
		// the Login page. After a successful login you can call a Web API.
		[HttpGet]
		public ActionResult Authorize()
		{
			var claims = new ClaimsPrincipal(User).Claims.ToArray();
			var identity = new ClaimsIdentity(claims, "Bearer");
			AuthenticationManager.SignIn(identity);
			return new EmptyResult();
		}

		//
		// GET: /Account/Login
		[AllowAnonymous]
		public ActionResult Login(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		//
		// POST: /Account/Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// This doesn't count login failures towards account lockout
			// To enable password failures to trigger account lockout, change to shouldLockout: true
			var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
			switch (result)
			{
				case SignInStatus.Success:
					return RedirectToLocal(returnUrl);
				case SignInStatus.LockedOut:
					return View("Lockout");
				case SignInStatus.RequiresVerification:
					return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
				case SignInStatus.Failure:
				default:
					ModelState.AddModelError("", "Invalid login attempt.");
					return View(model);
			}
		}

		//
		// GET: /Account/Register
		[AllowAnonymous]
		public ActionResult Register()
		{
			return View();
		}

		//
		// POST: /Account/Register
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				// get default gravatar image url
				var imageUrl = string.Format("http://www.gravatar.com/avatar/{0}?r=pg", GetEmailHash(model.Email.Trim().ToLower()));

				var user = new User { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, ImageUrl = imageUrl };
				var result = await UserManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					var currentUser = UserManager.FindByEmail(user.Email);

					var roleresult = UserManager.AddToRole(currentUser.Id, "Guest");

					await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

					// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
					// Send an email with this link
					// string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
					// var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
					// await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

					// send email to admin
					var adminUserEmail = ConfigurationManager.AppSettings["AdminUserEmail"];
					if (!string.IsNullOrEmpty(adminUserEmail))
					{
						var adminUser = await UserManager.FindByEmailAsync(adminUserEmail);
						if (adminUser != null)
						{
							await UserManager.SendEmailAsync(adminUser.Id, "NFL Pool - New user registration", $"A new user registered with email {user.Email}");
						}
					}

					return RedirectToAction("Index", "Home");
				}
				AddErrors(result);
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		//
		// GET: /Account/ConfirmEmail
		[AllowAnonymous]
		public async Task<ActionResult> ConfirmEmail(string userId, string code)
		{
			if (userId == null || code == null)
			{
				return View("Error");
			}
			var result = await UserManager.ConfirmEmailAsync(userId, code);
			return View(result.Succeeded ? "ConfirmEmail" : "Error");
		}

		//
		// GET: /Account/ForgotPassword
		[AllowAnonymous]
		public ActionResult ForgotPassword()
		{
			return View();
		}

		//
		// POST: /Account/ForgotPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await UserManager.FindByNameAsync(model.Email);
				if (user == null) // || !(await UserManager.IsEmailConfirmedAsync(user.Id)))   //-- if we require confirmation uncomment this code
				{
					// Don't reveal that the user does not exist or is not confirmed
					return View("ForgotPasswordConfirmation");
				}

				// For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
				// Send an email with this link
				string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
				var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
				await UserManager.SendEmailAsync(user.Id, "NFL Pool Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
				return RedirectToAction("ForgotPasswordConfirmation", "Account");
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		//
		// GET: /Account/ForgotPasswordConfirmation
		[AllowAnonymous]
		public ActionResult ForgotPasswordConfirmation()
		{
			return View();
		}

		//
		// GET: /Account/ResetPassword
		[AllowAnonymous]
		public ActionResult ResetPassword(string code)
		{
			return code == null ? View("Error") : View();
		}

		//
		// POST: /Account/ResetPassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var user = await UserManager.FindByNameAsync(model.Email);
			if (user == null)
			{
				// Don't reveal that the user does not exist
				return RedirectToAction("ResetPasswordConfirmation", "Account");
			}
			var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
			if (result.Succeeded)
			{
				return RedirectToAction("ResetPasswordConfirmation", "Account");
			}
			AddErrors(result);
			return View();
		}

		//
		// GET: /Account/ResetPasswordConfirmation
		[AllowAnonymous]
		public ActionResult ResetPasswordConfirmation()
		{
			return View();
		}

		//
		// POST: /Account/LogOff
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
			return RedirectToAction("Index", "Home");
		}

		#endregion

		[AllowAnonymous]
		[HttpGet]
		public ActionResult KeepAlive()
		{
			return Json(null, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public ActionResult List()
		{
			return View();
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public ActionResult ListData()
		{
			return Json(new { data = UserListViewModel.GetList() }, JsonRequestBehavior.AllowGet);
        }

		#region user list actions

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public ActionResult Edit(string id)
		{
			#region validation

			if (string.IsNullOrEmpty(id))
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid user ID");
			}
			var user = UserManager.FindById(id);
			if (user == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User not found");
			}

			#endregion

			var model = new UserEditViewModel
			{
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName
			};
			return View(model);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult> Edit(UserEditViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var user = await UserManager.FindByIdAsync(model.Id);
			if (user == null)
			{
				// Don't reveal that the user does not exist
				AddErrors(new IdentityResult("User not found"));
				return View(model);
			}
			// make sure email is not taken
			var otherUser = await UserManager.FindByEmailAsync(model.Email);
			if (otherUser != null && otherUser.Id != model.Id)
			{
				AddErrors(new IdentityResult("Email is already taken"));
				return View(model);
			}

			user.Email = model.Email;
			user.UserName = model.Email;
			user.FirstName = model.FirstName;
			user.LastName = model.LastName;

			var result = await UserManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				return RedirectToAction("List");
			}
			AddErrors(result);
			return View(model);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public ActionResult SecurityUp(string id)
		{
			#region validate

			var user = UserManager.FindById(id);
			if (user == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User not found");
			}

			// we expect the user to be in only one role
			var roles = UserManager.GetRoles(id);
			if (roles == null || roles.Count != 1)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid user role");
			}

			var role = roles[0];
			if (role == "Admin")
			{
				return Json(null);
			}

			#endregion

			UserManager.RemoveFromRole(id, role);
			UserManager.AddToRole(id, (role == "Guest") ? "User" : "Admin");

			if (role == "Guest")
			{
				// when going up from Guest to User send email about payment
				UserManager.SendEmail(id, "NFL Pool Payment Received", "Your payment has been received and your account is now active. Please use your email and password to access the site <a href=\"http://football.arashamini.com/\">http://football.arashamini.com/</a>");
			}

			return Json(null);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public ActionResult SecurityDown(string id)
		{
			#region validate

			var user = UserManager.FindById(id);
			if (user == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User not found");
			}

			// we expect the user to be in only one role
			var roles = UserManager.GetRoles(id);
			if (roles == null || roles.Count != 1)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid user role");
			}

			var role = roles[0];
			if (role == "Guest")
			{
				return Json(null);
			}

			#endregion

			UserManager.RemoveFromRole(id, role);
			UserManager.AddToRole(id, (role == "Admin") ? "User" : "Guest");

			return Json(null);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public ActionResult Delete(string id)
		{
			#region validate

			var userId = User.Identity.GetUserId();
			if (userId == id)
			{
				// cannot delete myself
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You cannot delete yourself");
			}

			var user = UserManager.FindById(id);
			if (user == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User not found");
			}

			#endregion

			// delete the user's picks
			PickViewModel.DeleteForUser(id);

			UserManager.Delete(user);

			return Json(null);
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public ActionResult SetPassword(string id)
		{
			#region validation

			if (string.IsNullOrEmpty(id))
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid user ID");
			}
			var user = UserManager.FindById(id);
			if (user == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User not found");
			}

			#endregion

			ViewBag.UserName = user.FirstName + " " + user.LastName;

			var model = new ResetPasswordViewModel
			{
				Email = user.Email,
				Code = UserManager.GeneratePasswordResetToken(id)
			};
            return View(model);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> SetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var user = await UserManager.FindByNameAsync(model.Email);
			if (user == null)
			{
				// Don't reveal that the user does not exist
				return RedirectToAction("ResetPasswordConfirmation", "Account");
			}
			var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
			if (result.Succeeded)
			{
				return RedirectToAction("List");
			}
			AddErrors(result);
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public ActionResult Winnings(string id, double winnings)
		{
			#region validation


			var user = UserManager.FindById(id);
			if (user == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User not found");
			}

			#endregion

			user.Winnings = winnings;
			UserManager.Update(user);

			return Json(null);
		}

		#endregion

		#region Helpers
		// Used for XSRF protection when adding external logins
		private const string XsrfKey = "XsrfId";

		private IAuthenticationManager AuthenticationManager
		{
			get
			{
				return HttpContext.GetOwinContext().Authentication;
			}
		}

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error);
			}
		}

		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			return RedirectToAction("Index", "Home");
		}

		private string GetEmailHash(string email)
		{
			byte[] hash;
			using (var md5 = System.Security.Cryptography.MD5.Create())
			{
				hash = md5.ComputeHash(Encoding.UTF8.GetBytes(email));
			}

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("x2"));
			}

			return sb.ToString();
		}

		#endregion
	}
}
