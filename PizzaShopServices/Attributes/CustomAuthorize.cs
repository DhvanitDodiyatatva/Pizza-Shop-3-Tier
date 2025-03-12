using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace PizzaShopServices.Attributes // Adjust namespace as needed
{
    public class CustomAuthorize : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public CustomAuthorize(string roles)
        {
            _roles = roles.Split(',').Select(r => r.Trim()).ToArray();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the user is authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // Check if the user has any of the required roles (if roles are specified)
            if (_roles.Length > 0 && !_roles.Any(role => context.HttpContext.User.IsInRole(role)))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}