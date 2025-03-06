using PizzaShopRepository.Models;
using PizzaShopRepository.Repositories;
using PizzaShopRepository.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace PizzaShopRepository.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public IEnumerable<Role> GetAllRoles()
        {
            return _roleRepository.GetAllRoles();
        }

        public RolePermissionVM GetRolePermissions(int roleId)
        {
            var role = _roleRepository.GetRoleWithPermissions(roleId);
            if (role == null)
            {
                return null;
            }

            var allPermissions = _roleRepository.GetAllPermissions();

            var viewModel = new RolePermissionVM
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Permissions = allPermissions.Select(p => new PermissionDetail
                {
                    PermissionId = p.Id,
                    PermissionName = p.Name,
                    CanView = role.RolePermissions
                        .FirstOrDefault(rp => rp.PermissionId == p.Id)?.CanView ?? false,
                    CanAddEdit = role.RolePermissions
                        .FirstOrDefault(rp => rp.PermissionId == p.Id)?.CanAddEdit ?? false,
                    CanDelete = role.RolePermissions
                        .FirstOrDefault(rp => rp.PermissionId == p.Id)?.CanDelete ?? false
                }).ToList()
            };

            return viewModel;
        }

        public void UpdateRolePermissions(RolePermissionVM model)
        {
            var existingPermissions = _roleRepository.GetRoleWithPermissions(model.RoleId)
                .RolePermissions;

            _roleRepository.RemoveRolePermissions(existingPermissions);

            var newPermissions = model.Permissions
                .Where(p => p.IsSelected)
                .Select(p => new RolePermission
                {
                    RoleId = model.RoleId,
                    PermissionId = p.PermissionId,
                    CanView = p.CanView,
                    CanAddEdit = p.CanAddEdit,
                    CanDelete = p.CanDelete
                });

            _roleRepository.AddRolePermissions(newPermissions);
            _roleRepository.SaveChanges();
        }
    }
}