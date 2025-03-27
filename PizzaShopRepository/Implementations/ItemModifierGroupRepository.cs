using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopRepository.Repositories
{
    public class ItemModifierGroupRepository : IItemModifierGroupRepository
    {
        private readonly PizzaShopContext _context;

        public ItemModifierGroupRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public async Task AddItemModifierGroupAsync(ItemModifierGroup itemModifierGroup)
        {
            // Check for existing tracked entity and detach it if necessary
            var existingEntity = _context.ItemModifierGroups
                .Local
                .FirstOrDefault(e => e.ItemId == itemModifierGroup.ItemId && e.ModifierGroupId == itemModifierGroup.ModifierGroupId);

            if (existingEntity != null)
            {
                _context.Entry(existingEntity).State = EntityState.Detached;
            }

            _context.ItemModifierGroups.Add(itemModifierGroup);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ItemModifierGroup>> GetItemModifierGroupsByItemIdAsync(int itemId)
        {
            return await _context.ItemModifierGroups
                .Include(img => img.ModifierGroup)
                .ThenInclude(mg => mg.ModifierGroupMappings)
                .ThenInclude(mgm => mgm.Modifier)
                .Where(img => img.ItemId == itemId && !img.IsDeleted)
                .ToListAsync();
        }

        public async Task UpdateItemModifierGroupAsync(ItemModifierGroup itemModifierGroup)
        {
            _context.ItemModifierGroups.Update(itemModifierGroup);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemModifierGroupAsync(int itemId, int modifierGroupId)
        {
            var itemModifierGroup = await _context.ItemModifierGroups
                .FirstOrDefaultAsync(img => img.ItemId == itemId && img.ModifierGroupId == modifierGroupId && !img.IsDeleted);
            if (itemModifierGroup != null)
            {
                itemModifierGroup.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}