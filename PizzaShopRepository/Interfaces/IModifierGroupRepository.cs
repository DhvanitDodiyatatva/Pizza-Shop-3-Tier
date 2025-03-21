using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface IModifierGroupRepository
{
    Task<List<ModifierGroup>> GetAllModifierGroupsAsync();
}
