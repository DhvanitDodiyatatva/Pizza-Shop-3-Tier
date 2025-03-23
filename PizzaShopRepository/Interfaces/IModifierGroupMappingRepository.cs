using PizzaShopRepository.Models;
using System.Collections.Generic;

namespace PizzaShopRepository.Repositories
{
    public interface IModifierGroupMappingRepository
    {

        Task<ModifierGroupMapping> CreateMappingAsync(int modifierGroupId, int modifierId);
        Task<List<ModifierGroupMapping>> GetMappingsForModifierAsync(int modifierId);
        Task<List<Modifier>> GetModifiersByModifierGroupIdAsync(int modifierGroupId);
        Task UpdateMappingAsync(ModifierGroupMapping mapping);
        Task<List<ModifierGroupMapping>> GetMappingsForModifierGroupAsync(int modifierGroupId);
    }
}
