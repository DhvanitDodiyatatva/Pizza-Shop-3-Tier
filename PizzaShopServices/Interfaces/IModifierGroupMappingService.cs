using PizzaShopRepository.Models;
using System.Collections.Generic;

namespace PizzaShopService
{
    public interface IModifierGroupMappingService
    {
        Task<ModifierGroupMapping> CreateMappingAsync(int modifierGroupId, int modifierId);
        Task<List<Modifier>> GetModifiersByGroupIdAsync(int modifierGroupId);
    }
}
