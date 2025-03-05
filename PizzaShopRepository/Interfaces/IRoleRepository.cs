// using PizzaShopRepository.Models;

// namespace PizzaShopRepository.Interfaces
// {
//     public interface IRoleRepository
//     {
//         Task<List<Role>> GetAllUserRolesAsync();
//     }
// }

using PizzaShopRepository.Models;
using System.Collections.Generic;

namespace PizzaShopRepository.Repositories
{
    public interface IRoleRepository
    {
        IEnumerable<Role> GetAllRoles();
        Role GetRoleWithPermissions(int roleId);
        void RemoveRolePermissions(IEnumerable<RolePermission> rolePermissions);
        void AddRolePermissions(IEnumerable<RolePermission> rolePermissions);
        IEnumerable<Permission> GetAllPermissions();
        void SaveChanges();
    }
}