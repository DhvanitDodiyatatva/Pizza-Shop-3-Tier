using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface IUserRepository
{
        Task<User?> GetUserByEmailAsync(string email);
}
