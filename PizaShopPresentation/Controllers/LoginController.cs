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
        public async Task<IActionResult> SendEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewData["ErrorMessage"] = "Email field is required.";
                return View("ForgotPassword");
            }

            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    ViewData["SuccessMessage"] = "If your email is registered, you'll receive a reset link.";
                    return View("ForgotPassword");
                }

                string resetPasswordUrl = Url.Action("ResetPassword", "Login", null, protocol: Request.Scheme);
                await _emailService.SendResetPasswordEmailAsync(email, resetPasswordUrl);
                ViewData["SuccessMessage"] = "If your email is registered, you'll receive a reset link.";
            }
            catch (DbUpdateException ex)
            {
                // Log the inner exception for debugging
                Console.WriteLine($"DbUpdateException: {ex.InnerException?.Message}");
                ViewData["ErrorMessage"] = "Failed to send email due to a database error. Please try again later.";
            }
            catch (Exception ex)
            {
                // Log the full exception
                Console.WriteLine($"Exception: {ex.Message}, Inner: {ex.InnerException?.Message}");
                ViewData["ErrorMessage"] = "Failed to send email. Error: " + ex.Message;
            }

            return View("ForgotPassword");
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