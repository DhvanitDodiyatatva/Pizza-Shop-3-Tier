using PizzaShopService.Interfaces;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;

namespace RmsServices.Implementations;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<Role>> GetUserAllUserRolesAsync()
    {
        return await _roleRepository.GetAllUserRolesAsync();
    }
}

