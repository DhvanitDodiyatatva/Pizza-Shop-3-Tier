using PizzaShopRepository.Models;
using System.Threading.Tasks;

namespace PizzaShopRepository.Interfaces
{
        public interface IUserRepository
        {
                Task<User?> GetUserByEmailAsync(string email);
                Task UpdateUserAsync(User user);
        }
}