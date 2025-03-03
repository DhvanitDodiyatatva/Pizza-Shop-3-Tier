using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;

namespace PizaShopPresentation.Controllers;

public class LoginController : Controller
{
    private readonly IUserService _userService;

    public LoginController(IUserService userService)
    {
        _userService = userService;
    }
    // public readonly AuthenticateService _ATH;

    // public LoginController(AuthenticateService Ath){
    //     _ATH = Ath;
    // }
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
            // Delegate authentication to the business layer
            var (token, expireHours) = await _userService.ValidateUserAsync(model);
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
            // Redirect to the provided returnUrl if valid; otherwise, redirect to the dashboard
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Dashboard", "Home");
        }
        catch (Exception ex)
        {
            // Use the exception message as the error message
            ViewData["ErrorMessage"] = ex.Message;
            return View("Index", model);
        }

    }
}