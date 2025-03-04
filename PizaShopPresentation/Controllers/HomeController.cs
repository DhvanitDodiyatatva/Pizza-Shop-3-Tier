using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizaShopPresentation.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PizzaShopPresentation.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserCrudService _userCrudService;

        

        public HomeController(ILogger<HomeController> logger, IUserCrudService userCrudService)
        {
            _logger = logger;
            _userCrudService = userCrudService;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Users(string searchQuery = "", int page = 1, int pageSize = 5)
        {
            var (users, totalUsers) = await _userCrudService.GetUsersAsync(searchQuery, page, pageSize);

            int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalUsers > 0) page = totalPages;

            ViewBag.TotalUsers = totalUsers;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = searchQuery;

            return View(users);
        }

        public IActionResult Roles()
        {
            return View();
        }

        public async Task<IActionResult> MyProfile()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Login");
            }

            var model = await _userCrudService.GetUserProfileAsync(email);
            if (model == null)
            {
                return NotFound("User not found.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([Bind("FirstName,LastName,Username,Phone,Country,State,City,Address,Zipcode")] MyProfile model)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                await _userCrudService.UpdateUserProfileAsync(email, model);
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed to update profile.";
            }

            return RedirectToAction("MyProfile");
        }

        public IActionResult Logout()
        {
            if (Request.Cookies["JWT"] != null)
            {
                Response.Cookies.Delete("RememberEmail");
                Response.Cookies.Delete("RememberPassword");
                Response.Cookies.Delete("RememberMeChecked");
            }
            return RedirectToAction("Index", "Login");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                await _userCrudService.ChangePasswordAsync(email, model);
                TempData["SuccessMessage"] = "Password changed successfully.";
                Response.Cookies.Delete("RememberEmail");
                Response.Cookies.Delete("RememberPassword");
                Response.Cookies.Delete("RememberMeChecked");
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult AddNewUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNewUser(AddEditUserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _userCrudService.AddNewUserAsync(model);
                TempData["SuccessMessage"] = "User added successfully!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var model = await _userCrudService.GetUserForEditAsync(id);
            if (model == null)
            {
                return NotFound("User not found.");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([Bind("Id,FirstName,LastName,Username,Phone,Country,State,City,Address,Zipcode,Status,Role")] AddEditUserVM model, IFormFile ProfileImg)
        {
            try
            {
                await _userCrudService.UpdateUserAsync(model, ProfileImg, HttpContext.Request.Host.Value);
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to update profile: " + ex.Message;
            }

            return RedirectToAction("EditUser", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userCrudService.SoftDeleteUserAsync(id);
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to delete user: " + ex.Message;
            }

            return RedirectToAction("Users");
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}