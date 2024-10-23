using BlossomCart.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.Net;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace BlossomCart.Controllers
{
	public class AuthController : Controller
	{
		private readonly BlossomCartsContext db;
		public AuthController(BlossomCartsContext _db)
		{
			db = _db;
		}
		public IActionResult Signup()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Signup(Signup user)
		{
			var checkExistingUser = db.Signups.FirstOrDefault(o => o.EMail == user.EMail );
			if (checkExistingUser != null)
			{
				ViewBag.msg = "User already registered";
				return View();
			}
			else
			{

				var hasher = new PasswordHasher<string>();
				user.Password = hasher.HashPassword(user.EMail, user.Password);

				db.Signups.Add(user);
				db.SaveChanges();
				TempData["UserInsert"] = "Successfully Register. Login Now..!";


				return RedirectToAction("Login");

			}
		}
		public IActionResult Login()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Login(Signup user1)
		{
			bool IsAuthenticated = false;
			string controller = "";
			ClaimsIdentity identity = null;

			var checkUser = db.Signups.FirstOrDefault(u1 => u1.EMail == user1.EMail);
			if (checkUser != null)
			{
				var hasher = new PasswordHasher<string>();
				var verifyPass = hasher.VerifyHashedPassword(user1.EMail, checkUser.Password, user1.Password);

				if (verifyPass == PasswordVerificationResult.Success && checkUser.Role == 1)
				{
					identity = new ClaimsIdentity(new[]
					{
					new Claim(ClaimTypes.Name ,checkUser.CustamerName),

					new Claim(ClaimTypes.Role ,"user"),
				}
				   , CookieAuthenticationDefaults.AuthenticationScheme);

					HttpContext.Session.SetInt32("id", checkUser.Id);
					HttpContext.Session.SetString("email", checkUser.EMail);
					HttpContext.Session.SetString("username", checkUser.CustamerName);
                    HttpContext.Session.SetString("number", checkUser.PhoneNumber.ToString());




                    IsAuthenticated = true;
					controller = "Home";
				}
				
                else
				{
					IsAuthenticated = false;

				}
				if (IsAuthenticated)
				{
					var principal = new ClaimsPrincipal(identity);

					var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

					return RedirectToAction("Index", controller);
				}

				else
				{
					ViewBag.msg = "Wrong Password";
					return View();
				}
			}
			else
			{
				ViewBag.msg = "User not found";
				return View();
			}

		}
		public IActionResult Logout()
		{
			HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			HttpContext.Session.Remove("email");
			HttpContext.Session.Remove("username");
			return RedirectToAction("Index", "Home");

		}

		public IActionResult AdminLogin()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AdminLogin(string email, string pass)
		{
			bool IsAuthenticated = false;
			string controller = "";
			ClaimsIdentity identity = null;

			var checkUser = db.Signups.FirstOrDefault(u1 => u1.EMail == email);
			if (checkUser != null)
			{
				var hasher = new PasswordHasher<string>();
				var verifyPass = hasher.VerifyHashedPassword(email, checkUser.Password, pass);

				if (verifyPass == PasswordVerificationResult.Success && checkUser.Role == 2)
				{
					identity = new ClaimsIdentity(new[]
					{
					new Claim(ClaimTypes.Name ,checkUser.CustamerName),

					new Claim(ClaimTypes.Role ,"Admin"),
				}
				   , CookieAuthenticationDefaults.AuthenticationScheme);

					HttpContext.Session.SetInt32("id", checkUser.Id);
					HttpContext.Session.SetString("email", checkUser.EMail);
					HttpContext.Session.SetString("username", checkUser.CustamerName);
					HttpContext.Session.SetString("number", checkUser.PhoneNumber.ToString());




					IsAuthenticated = true;
					controller = "Home";
				}

				else
				{

					IsAuthenticated = false;
				}
				if (IsAuthenticated)
				{
					var principle = new ClaimsPrincipal(identity);
					var Login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principle);
					return RedirectToAction("Index", "Admin");
				}
				else
				{
					ViewBag.msg = "Wrong Password";
					return View();
				}

            }
           

            else
            {
                ViewBag.msg = "Admin not found";
                return View();
            }
        }
		public bool SendEmail(string email, string message, string subject)
		{


			SmtpClient client = new SmtpClient("smtp.gmail.com", 587);

			client.EnableSsl = true;
			client.UseDefaultCredentials = false;
			client.Credentials = new NetworkCredential("ahmedidrees247@gmail.com", "rxqj ubfd yiqq fmhv");

			MailMessage msg = new MailMessage("ahmedidrees247@gmail.com", email);
			msg.Subject = subject;

			msg.Body = message;


			// msg.Attachments.Add(new Attachment(PathToAttachment));
			client.Send(msg);


			return true;
		}
		public IActionResult Forgot()
		{

			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Forgot(string email)
		{
			var user = db.Signups.FirstOrDefault(m => m.EMail == email);
			if (user == null)
			{
				ViewBag.msg = "Invalid User Please Create an account first.";
				return View();
			}
			else
			{
				Guid guid = Guid.NewGuid();
				var token = guid.ToString();
				user.Token = token;
				db.Signups.Update(user);
				db.SaveChanges();

				string message = $"A request has been generated to reset your password. Please <a href=\"https://localhost:5016/Auth/ChangePassword?token={token}&email={email}\">Click here</a> to reset your password.";
				string subject = "Password Reset Request";
				if (SendEmail(email, message, subject))
				{
					ViewBag.msgSuccess = "Mail has been sent to you. Check Inbox";

				}
				else
				{
					ViewBag.msg = "Can't proceed your request. Try again later.";
				}
				return View();
			}
		}


            public IActionResult ChangePassword(string email,string token)
            {
                var user = db.Signups.FirstOrDefault(m => m.EMail == email && m.Token==token);
                if (user == null)
                {
                    TempData["msg"] = "Invalid URL Please Create an account first.";
                    return RedirectToAction("login");
                }
                else
                {

                ViewBag.token = token;
                ViewBag.email = email;
                return View();
                }



            }
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ChangePassword(string pass, string cpass, string token, string email)
        {
            var user = db.Signups.FirstOrDefault(m => m.EMail == email && m.Token == token);
            if (user == null)
            {
                TempData["msg"] = "Invalid URL Please Create an account first.";
                return RedirectToAction("login");
            }
            else
            {
				if (pass == cpass)
				{
                    Guid guid = Guid.NewGuid();
                    var newtoken = guid.ToString();

                    var hasher = new PasswordHasher<string>();
					user.Password = hasher.HashPassword(user.EMail, pass);
					user.Token = newtoken;
                    db.Signups.Update(user);
                    db.SaveChanges();
                    ViewBag.msgSuccess = "Password changed successfully. Go to login.";
					return View();

                }
				else
				{
                    ViewBag.msg = "Password and Confrim Password Not Same.";
                }

                return View();
            }



        }
    }
}
