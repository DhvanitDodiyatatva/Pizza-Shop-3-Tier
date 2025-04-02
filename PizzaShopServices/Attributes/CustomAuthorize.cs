// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;
// using System.Linq;

// namespace PizzaShopServices.Attributes
// {
//     public class CustomAuthorize : Attribute, IAuthorizationFilter
//     {
//         private readonly string[] _roles;

//         public CustomAuthorize(string roles)
//         {
//             _roles = roles.Split(',').Select(r => r.Trim()).ToArray();
//         }

//         public void OnAuthorization(AuthorizationFilterContext context)
//         {
//             // Check if the user is authenticated
//             if (!context.HttpContext.User.Identity.IsAuthenticated)
//             {
//                 context.Result = new RedirectToActionResult("Index", "Login", null);
//                 return;
//             }

//             // Check if the user has any of the required roles (if roles are specified)
//             if (_roles.Length > 0 && !_roles.Any(role => context.HttpContext.User.IsInRole(role)))
//             {
//                 context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
//             }
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PizzaShopRepository.Services;
using PizzaShopServices.Interfaces;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace PizzaShopServices.Attributes
{
    public enum PermissionType
    {
        View,
        Alter,
        Delete
    }

    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _moduleName;
        private readonly PermissionType _permissionType;
        private readonly string[] _roles;

        public CustomAuthorizeAttribute(string moduleName, PermissionType permissionType, params string[] roles)
        {
            _moduleName = moduleName;
            _permissionType = permissionType;
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var jwtService = context.HttpContext.RequestServices.GetService<IJwtService>();
            var roleService = context.HttpContext.RequestServices.GetService<IRoleService>();

            if (jwtService == null || roleService == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            var token = jwtService.GetJWTToken(context.HttpContext.Request);
            var principal = jwtService.ValidateToken(token);

            if (principal == null)
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            context.HttpContext.User = principal;

            // Check roles if specified
            if (_roles.Length > 0)
            {
                var userRole = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                    return;
                }
            }

            // Extract roleId from claims (assuming Role claim holds the role name, not ID)
            var roleName = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var role = roleService.GetAllRoles().FirstOrDefault(r => r.Name == roleName);
            if (role == null || !int.TryParse(role.Id.ToString(), out int roleId))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                return;
            }

            // Fetch permission for the role and module
            var permission = roleService.GetPermissionForRoleAndModule(roleId, _moduleName).GetAwaiter().GetResult();
            if (permission == null)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                return;
            }

            bool hasPermission = _permissionType switch
            {
                PermissionType.View => permission.CanView ?? false,
                PermissionType.Alter => permission.CanAddEdit ?? false,
                PermissionType.Delete => permission.CanDelete ?? false,
                _ => false
            };

            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
            }
        }
    }
}