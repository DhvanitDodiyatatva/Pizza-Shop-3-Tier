using PizzaShopRepository.Models;


namespace PizzaShopService.Interfaces
{
    public interface IRoleService
    {
        Task<List<Role>> GetUserAllUserRolesAsync();
    }
}



