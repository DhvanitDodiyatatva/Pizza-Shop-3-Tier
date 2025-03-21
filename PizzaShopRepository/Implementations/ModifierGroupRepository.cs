using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopRepository.Repositories
{
    public class ModifierGroupRepository : IModifierGroupRepository
    {
        private readonly PizzaShopContext _context;

        public ModifierGroupRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public async Task<List<ModifierGroup>> GetAllModifierGroupsAsync()
        {
            return await _context.ModifierGroups.Where(mg => !mg.IsDeleted).OrderBy(mg => mg.Id).ToListAsync();
        }


    }
}