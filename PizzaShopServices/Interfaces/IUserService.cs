using PizzaShopRepository.ViewModels;
using System.Threading.Tasks;

namespace PizzaShopServices.Interfaces
{
        public interface IUserService
        {
                Task<(string Token, double ExpireHours)> ValidateUserAsync(Authenticate model);
                Task ResetPasswordAsync(ResetPassword model);
        }
}