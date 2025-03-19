using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface IModifierRepository
{
    // Task<List<Modifier>> GetModifiersByModifierGrpAsync (int modifierGroupId);
    Task<List<Modifier>> GetAllModifiersAsync();
    Task<Modifier?> GetModifierByIdAsync(int id);

}
