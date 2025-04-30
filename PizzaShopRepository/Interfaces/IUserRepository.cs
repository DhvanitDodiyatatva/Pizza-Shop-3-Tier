using PizzaShopRepository.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaShopRepository.Interfaces
{
        public interface IUserRepository
        {
                Task<User?> GetUserByEmailAsync(string email);
                Task<User?> GetUserByIdAsync(int id);
                Task<IQueryable<User>> GetUsersQueryableAsync();
                Task<bool> UserExistsAsync(string username, string email);
                Task AddUserAsync(User user);
                Task UpdateUserAsync(User user);

                
        }
}