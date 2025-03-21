using PizzaShopRepository.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using PizzaShopRepository.Data;

namespace PizzaShopRepository.Repositories
{
    public class ModifierGroupMappingRepository : IModifierGroupMappingRepository
    {
        private readonly PizzaShopContext _context;

        public ModifierGroupMappingRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public async Task<ModifierGroupMapping> CreateMappingAsync(int modifierGroupId, int modifierId)
        {
            var mapping = new ModifierGroupMapping
            {
                ModifierGroupId = modifierGroupId,
                ModifierId = modifierId
            };

            _context.ModifierGroupMappings.Add(mapping);
            await _context.SaveChangesAsync();
            return mapping;
        }

        public async Task<List<ModifierGroupMapping>> GetMappingsForModifierAsync(int modifierId)
        {
            return await _context.ModifierGroupMappings
                        .Where(m => m.ModifierId == modifierId)
                        .ToListAsync();
        }

        public async Task<List<Modifier>> GetModifiersByModifierGroupIdAsync(int modifierGroupId)
        {
            return await _context.ModifierGroupMappings
                        .Where(m => m.ModifierGroupId == modifierGroupId && !m.IsDeleted)
                        .Select(m => m.Modifier)
                        .ToListAsync();
        }
    }
}
