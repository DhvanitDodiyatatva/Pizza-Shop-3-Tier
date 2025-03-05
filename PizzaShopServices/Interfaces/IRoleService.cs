// using PizzaShopRepository.Models;


// namespace PizzaShopService.Interfaces
// {
//     public interface IRoleService
//     {
//         Task<List<Role>> GetUserAllUserRolesAsync();
//     }
// }

using PizzaShopRepository.ViewModels;
using System.Collections.Generic;

namespace PizzaShopRepository.Services
{
    public interface IRoleService
    {
        IEnumerable<Models.Role> GetAllRoles();
        RolePermissionVM GetRolePermissions(int roleId);
        void UpdateRolePermissions(RolePermissionVM model);
    }
}

