using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface IUserService
{
        Task<(string Token, double ExpireHours)> ValidateUserAsync(Authenticate model);

}
