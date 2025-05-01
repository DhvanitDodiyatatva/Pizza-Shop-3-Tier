using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using System.Threading.Tasks;

namespace PizzaShopServices.Interfaces
{
        public interface IUserService
        {
                Task<(string Token, double ExpireHours, bool Success, string Message)> ValidateUserAsync(Authenticate model);
                Task ResetPasswordAsync(ResetPassword model);
                Task<User> GetUserByEmailAsync(string email);
        }
}