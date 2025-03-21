using PizzaShopRepository.Models;

namespace PizzaShopServices.Interfaces;

public interface IModifierGroupService
{
    Task<List<ModifierGroup>> GetAllModifierGroupAsync();
}
