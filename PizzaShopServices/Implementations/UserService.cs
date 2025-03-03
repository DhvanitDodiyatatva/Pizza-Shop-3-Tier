using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;

namespace PizzaShopServices.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _authRepository;
    private readonly IConfiguration _configuration;

    public UserService(IUserRepository authRepository, IConfiguration configuration)
    {
        _authRepository = authRepository;
        _configuration = configuration;
    }
    public async Task<(string Token, double ExpireHours)> ValidateUserAsync(Authenticate model)
    {
        // Retrieve the user using the repository
        var user = await _authRepository.GetUserByEmailAsync(model.Email);
        if (user == null)
        {
            throw new Exception("User does not exist.");
        }

        // Validate password using BCrypt
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
        if (!isPasswordValid)
        {
            throw new Exception("Incorrect password.");
        }

        // Create the JWT token
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
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


}

