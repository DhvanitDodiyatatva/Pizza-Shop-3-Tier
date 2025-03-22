using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface IModifierService
{

    Task<List<Modifier>> GetAllModifiersAsync();
    Task<ModifierViewModel> AddModifierAsync(ModifierViewModel model);
    Task<ModifierViewModel?> GetModifierForEditAsync(int id);
    Task<ModifierViewModel> UpdateModifierAsync(ModifierViewModel model);


}