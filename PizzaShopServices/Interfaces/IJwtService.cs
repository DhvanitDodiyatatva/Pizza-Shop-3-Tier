using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PizzaShopServices.Interfaces
{
    public interface IJwtService
    {
        string GetJWTToken(HttpRequest request);
        ClaimsPrincipal ValidateToken(string token);
    }
}