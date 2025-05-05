using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PizaShopPresentation.Models;
using PizzaShopRepository.Models;
using PizzaShopRepository.Repositories;
using PizzaShopRepository.Services;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Attributes;

using PizzaShopServices.Interfaces;
using System.Diagnostics;

using System.Threading.Tasks;

namespace PizzaShopPresentation.Controllers
{

    // [CustomAuthorize("super_admin, chef, account_manager")]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserCrudService _userCrudService;

        // private readonly IRoleService _roleService;
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleService _roleService;

        private readonly PizzaShopRepository.Data.PizzaShopContext _context;


        public HomeController(ILogger<HomeController> logger, IUserCrudService userCrudService, IRoleService roleService, IRoleRepository roleRepository, PizzaShopRepository.Data.PizzaShopContext context)


        {
            _logger = logger;
            _userCrudService = userCrudService;
            _roleService = roleService;
            _roleRepository = roleRepository;
            _roleService = roleService;
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [CustomAuthorize("Users", PermissionType.View, "super_admin", "account_manager", "chef")]
        public async Task<IActionResult> Users(string searchQuery = "", int page = 1, int pageSize = 5, string sortBy = "Id", string sortOrder = "asc")
        {
            var (users, totalUsers) = await _userCrudService.GetUsersAsync(searchQuery, page, pageSize, sortBy, sortOrder);

            int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalUsers > 0) page = totalPages;

            ViewBag.TotalUsers = totalUsers;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentUserEmail = User.Identity?.Name;

            return View(users);
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
        public async Task<IActionResult> UpdateProfile(MyProfile model, IFormFile? ImageFile)
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                model.ImageFile = ImageFile; // Bind the uploaded file to the model
                await _userCrudService.UpdateUserProfileAsync(email, model);
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("MyProfile");
            }


        }

        public IActionResult Logout()
        {


            Response.Cookies.Delete("JWT");
            Response.Cookies.Delete("RememberEmail");
            Response.Cookies.Delete("RememberPassword");
            Response.Cookies.Delete("RememberMeChecked");

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
                Response.Cookies.Delete("JWT");
                Response.Cookies.Delete("RememberEmail");
                Response.Cookies.Delete("RememberPassword");
                Response.Cookies.Delete("RememberMeChecked");
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
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
                TempData["ErrorMessage"] = ex.Message;
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
        public async Task<IActionResult> UpdateUser([Bind("Id,FirstName,LastName,Username,Phone,Country,State,City,Address,Zipcode,Status,Role,ProfileImage,RemoveImage")] AddEditUserVM model, IFormFile ImageFile)
        {
            try
            {
                await _userCrudService.UpdateUserAsync(model, ImageFile, HttpContext.Request.Host.Value);
                TempData["SuccessMessage"] = "User Edited successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to Edit User: " + ex.Message;
                return RedirectToAction("EditUser", new { id = model.Id });

            }

            return RedirectToAction("Users", new { id = model.Id });
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


        [CustomAuthorize("RoleAndPermission", PermissionType.View, "super_admin", "account_manager", "chef")]
        public IActionResult Roles()
        {
            var roles = _roleService.GetAllRoles();
            return View(roles);
        }

        public IActionResult Permissions(int roleId)
        {
            var viewModel = _roleService.GetRolePermissions(roleId);
            if (viewModel == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UpdatePermissions(RolePermissionVM model, [FromForm] string changedPermissions)
        {
            if (!ModelState.IsValid)
            {
                return View("Permissions", model);
            }

            if (!string.IsNullOrEmpty(changedPermissions))
            {
                var changedPerms = System.Text.Json.JsonSerializer.Deserialize<List<ChangedPermission>>(changedPermissions);
                if (changedPerms != null)
                {
                    var role = _roleRepository.GetRoleWithPermissions(model.RoleId);
                    if (role != null)
                    {
                        foreach (var change in changedPerms)
                        {
                            var permission = model.Permissions[change.index];
                            var rolePermission = role.RolePermissions
                                .FirstOrDefault(rp => rp.PermissionId == permission.PermissionId);
                            if (rolePermission != null)
                            {
                                rolePermission.CanView = change.canView;
                                rolePermission.CanAddEdit = change.canAddEdit;
                                rolePermission.CanDelete = change.canDelete;
                                if (!change.isSelected)
                                {
                                    _context.RolePermissions.Remove(rolePermission);
                                }
                            }
                            else if (change.isSelected)
                            {
                                _context.RolePermissions.Add(new RolePermission
                                {
                                    RoleId = model.RoleId,
                                    PermissionId = permission.PermissionId,
                                    CanView = change.canView,
                                    CanAddEdit = change.canAddEdit,
                                    CanDelete = change.canDelete
                                });
                            }
                        }
                        _roleRepository.SaveChanges();
                    }
                }
            }

            return Json(new { success = true });
        }

        // Add a new class to deserialize the changed permissions
        public class ChangedPermission
        {
            public int index { get; set; }
            public bool isSelected { get; set; }
            public bool canView { get; set; }
            public bool canAddEdit { get; set; }
            public bool canDelete { get; set; }
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        public async Task<IActionResult> MyProfileOA()
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
        public async Task<IActionResult> UpdateProfileOA([Bind("FirstName,LastName,Username,Phone,Country,State,City,Address,Zipcode")] MyProfile model)
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


        public IActionResult ChangePasswordOA()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePasswordOA(ChangePassword model)
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
                Response.Cookies.Delete("JWT");
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
    }
}