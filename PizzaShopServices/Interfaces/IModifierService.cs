using PizzaShopRepository.Models;

namespace PizzaShopServices.Interfaces;

public interface IModifierService
{
    Task<List<Modifier>> GetModifiersByModifierGrpAsync (int modifierGroupId);
    Task<List<Modifier>> GetAllModifiersAsync();
    Task<Modifier?> GetModifierByIdAsync(int id);
}