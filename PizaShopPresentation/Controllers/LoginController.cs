using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using System.Threading.Tasks;
using PizzaShopServices.Attributes;
using Microsoft.EntityFrameworkCore;

namespace PizzaShopPresentation.Controllers
{

    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService; // New service for email functionality

        public LoginController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        // GET: Login Page
        public IActionResult Index()
        {
            var model = new Authenticate();

            if (Request.Cookies.ContainsKey("RememberEmail"))
            {
                model.Email = Request.Cookies["RememberEmail"];
                ViewData["RememberMeChecked"] = true;

                if (Request.Cookies.ContainsKey("RememberPassword"))
                {
                    model.Password = Request.Cookies["RememberPassword"];
                }
            }
            else
            {
                ViewData["RememberMeChecked"] = false;
            }

            return View(model);
        }

        // POST: Login
        [Route("", Name = "LoginValidation")]
        [HttpPost]
        public async Task<IActionResult> Method(Authenticate model, bool RememberMe, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {

                var (token, expireHours, Success, Message) = await _userService.ValidateUserAsync(model);

                // Handle "Remember Me" functionality with cookies
                if (RememberMe)
                {
                    CookieOptions options = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(30),
                        HttpOnly = false, // Allow JavaScript access (Less secure)
                        Secure = false    // Set to true in production with HTTPS
                    };
                    Response.Cookies.Append("RememberEmail", model.Email, options);
                    Response.Cookies.Append("RememberPassword", model.Password, options);
                    Response.Cookies.Append("RememberMeChecked", "true", options);
                }
                else
                {
                    Response.Cookies.Delete("RememberEmail");
                    Response.Cookies.Delete("RememberPassword");
                    Response.Cookies.Delete("RememberMeChecked");
                }

                // Store the JWT in an HTTP-only cookie
                HttpContext.Response.Cookies.Append("JWT", token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddHours(expireHours)
                });

                // Check user role for redirection
                var user = await _userService.GetUserByEmailAsync(model.Email);
                if (user != null && user.Role == "chef")
                {
                    TempData["successMessage"] = "User Logged in successfully!";
                    return RedirectToAction("Kot", "OrderApp");
                }

                TempData["successMessage"] = "User Logged in successfully !";

                // Redirect to the provided returnUrl if valid; otherwise, redirect to the dashboard
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Dashboard", "Home");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View("Index", model);
            }
        }

        // GET: Forgot Password Page
        public IActionResult ForgotPassword(string? email)
        {
            ViewData["Email"] = email;
            return View();
        }

        // POST: Send Email
        [HttpPost]
        public async Task<IActionResult> SendEmail(Authenticate model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                ViewData["ErrorMessage"] = "Email field is required.";
                return View("ForgotPassword", model);
            }

            try
            {
                var user = await _userService.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    ViewData["ErrorMessage"] = "User is not registered.";
                    return View("ForgotPassword", model);
                }

                string resetPasswordUrl = Url.Action("ResetPassword", "Login", null, protocol: Request.Scheme);
                await _emailService.SendResetPasswordEmailAsync(model.Email, resetPasswordUrl);

                ViewData["SuccessMessage"] = "Email sent successfully.";
                return View("ForgotPassword", model);
            }
            catch (DbUpdateException ex)
            {

                ViewData["ErrorMessage"] = "Failed to send email due to a database error. Please try again later.";
            }
            catch (Exception ex)
            {

                ViewData["ErrorMessage"] = "Failed to send email. Error: " + ex.Message;
            }

            return View("ForgotPassword", model);
        }

        // GET: ResetPassword
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewData["ErrorMessage"] = "Invalid or missing token.";
                return RedirectToAction("ForgotPassword");
            }

            var user = await _userService.GetUserByResetTokenAsync(token);
            if (user == null || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
            {
                ViewData["ErrorMessage"] = "The reset link is invalid or has expired.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPassword { Email = user.Email };
            ViewData["Token"] = token; // Pass token to the view for form submission
            return View(model);
        }

        // POST: ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Token"] = model.Token;
                return View(model);
            }

            try
            {
                var user = await _userService.GetUserByResetTokenAsync(model.Token);
                if (user == null || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
                {
                    ViewData["ErrorMessage"] = "The reset link is invalid or has expired.";
                    return RedirectToAction("ForgotPassword");
                }

                await _userService.ResetPasswordAsync(model);

                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiry = null;
                await _userService.UpdateUserAsync(user);

                TempData["SuccessMessage"] = "Password reset successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewData["Token"] = model.Token;
                return View(model);
            }
        }
    }
}