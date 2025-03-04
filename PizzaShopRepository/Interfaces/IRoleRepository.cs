using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllUserRolesAsync();
    }
}

