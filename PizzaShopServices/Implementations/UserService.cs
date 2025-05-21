using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using System.Threading.Tasks;
using PizzaShopRepository.Models;
using PizzaShopRepository.Services;

namespace PizzaShopServices.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleService _roleService;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IRoleService roleService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _roleService = roleService;
            _configuration = configuration;

        }

        public async Task<(string Token, double ExpireHours, bool Success, string Message)> ValidateUserAsync(Authenticate model)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Email.ToLower().Trim());
            if (user == null)
            {
                throw new Exception("User does not exist.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
            if (!isPasswordValid)
            {
                throw new Exception("Incorrect password.");
            }

            if (user.Status != true)
            {
                throw new Exception("Invalid Username or Password.");
            }

            // Get role permissions
            var role = _roleService.GetAllRoles().FirstOrDefault(r => r.Name == user.Role);
            if (role == null)
            {
                throw new Exception("Role not found.");
            }

            // Define modules to include in claims
            var modules = new[] { "Menu", "TableAndSection", "TaxAndFee", "Users", "Order", "Customers" };
            var permissionClaims = new List<Claim>();

            foreach (var module in modules)
            {
                var permission = await _roleService.GetPermissionForRoleAndModule(role.Id, module);
                if (permission != null)
                {
                    if (permission.CanView == true)
                        permissionClaims.Add(new Claim($"Permission_{module}", "View"));
                    if (permission.CanAddEdit == true)
                        permissionClaims.Add(new Claim($"Permission_{module}", "Alter"));
                    if (permission.CanDelete == true)
                        permissionClaims.Add(new Claim($"Permission_{module}", "Delete"));
                }
            }

            // Create JWT token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("profile_image", user.ProfileImage ?? string.Empty)
            };
            claims.AddRange(permissionClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            double expireHours = _configuration.GetValue<double>("Jwt:ExpireHours");
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expireHours),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            try
            {
                return (tokenString, expireHours, true, "Login successful.");
            }
            catch (Exception ex)
            {
                return (tokenString, expireHours, false, ex.Message);
            }
        }

        public async Task ResetPasswordAsync(ResetPassword model)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                throw new Exception("Invalid request.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, 12);
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<User> GetUserByResetTokenAsync(string token)
        {
            return await _userRepository.GetUserByResetTokenAsync(token);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateUserAsync(user);
        }
    }
}