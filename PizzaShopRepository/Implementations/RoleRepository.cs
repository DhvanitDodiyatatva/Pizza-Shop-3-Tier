using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;

namespace PizzaShopRepository.Implementations
{

    public class RoleRepository : IRoleRepository
    {
        private readonly PizzaShopContext _context;

        public RoleRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllUserRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

    }
}