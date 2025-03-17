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

        public async Task<List<ModifierGroup>> GetAllModifierGrpAsync()
        {
            return await _context.ModifierGroups
                                 .Where(c => !c.IsDeleted)
                                 .ToListAsync();
        }

        public async Task<ModifierGroup?> GetModifierGrpByIdAsync(int id)
        {
            return await _context.ModifierGroups.FirstOrDefaultAsync(c => c.Id == id);
        }


    }
}