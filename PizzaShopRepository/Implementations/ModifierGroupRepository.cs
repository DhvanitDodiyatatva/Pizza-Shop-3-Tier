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

        public async Task<ModifierGroup?> GetModifierGroupByIdAsync(int id)
        {
            return await _context.ModifierGroups
                .FirstOrDefaultAsync(mg => mg.Id == id && !mg.IsDeleted);
        }

        public async Task AddModifierGroupAsync(ModifierGroup modifierGroup)
        {
            _context.ModifierGroups.Add(modifierGroup);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateModifierGroupAsync(ModifierGroup modifierGroup)
        {
            _context.ModifierGroups.Update(modifierGroup);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteModifierGroupAsync(int id)
        {
            var modifierGroup = await _context.ModifierGroups.FindAsync(id);
            if (modifierGroup != null)
            {
                modifierGroup.IsDeleted = true;
                _context.ModifierGroups.Update(modifierGroup);
                await _context.SaveChangesAsync();
            }
        }

    }
}