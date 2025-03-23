using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface IModifierGroupService
{
    Task<List<ModifierGroup>> GetAllModifierGroupAsync();
    Task<ModifierGroupViewModel> AddModifierGroupAsync(ModifierGroupViewModel model);
    Task<ModifierGroupViewModel?> GetModifierGroupForEditAsync(int id);
    Task<ModifierGroupViewModel> UpdateModifierGroupAsync(ModifierGroupViewModel model);
    Task DeleteModifierGroupAsync(int id);
}
