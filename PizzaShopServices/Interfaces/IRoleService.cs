using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using System.Collections.Generic;

namespace PizzaShopRepository.Services
{
    public interface IRoleService
    {
        IEnumerable<Models.Role> GetAllRoles();
        RolePermissionVM GetRolePermissions(int roleId);
        void UpdateRolePermissions(RolePermissionVM model);
        Task<RolePermission> GetPermissionForRoleAndModule(int roleId, string moduleName);
    }
}

