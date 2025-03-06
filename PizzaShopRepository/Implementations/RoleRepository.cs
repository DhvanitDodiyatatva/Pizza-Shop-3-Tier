using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Models;
using System.Collections.Generic;

namespace PizzaShopRepository.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly PizzaShopContext _context;

        public RoleRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public IEnumerable<Role> GetAllRoles()
        {
            return _context.Roles.ToList();
        }

        public Role GetRoleWithPermissions(int roleId)
        {
            return _context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefault(r => r.Id == roleId);
        }

        public void RemoveRolePermissions(IEnumerable<RolePermission> rolePermissions)
        {
            _context.RolePermissions.RemoveRange(rolePermissions);
        }

        public void AddRolePermissions(IEnumerable<RolePermission> rolePermissions)
        {
            _context.RolePermissions.AddRange(rolePermissions);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            return _context.Permissions.ToList();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}