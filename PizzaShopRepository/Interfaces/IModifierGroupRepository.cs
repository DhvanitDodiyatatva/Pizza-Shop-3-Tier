using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface IModifierGroupRepository
{
    Task<List<ModifierGroup>> GetAllModifierGroupsAsync();
    Task<ModifierGroup?> GetModifierGroupByIdAsync(int id);
    Task AddModifierGroupAsync(ModifierGroup modifierGroup);
    Task UpdateModifierGroupAsync(ModifierGroup modifierGroup);
    Task SoftDeleteModifierGroupAsync(int id);
}
