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

namespace PizzaShopServices.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<(string Token, double ExpireHours)> ValidateUserAsync(Authenticate model)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Email);
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
                throw new Exception("User is inactive .");
            }

            // Create JWT token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

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

            return (tokenString, expireHours);
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
    }
}