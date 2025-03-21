using PizzaShopRepository.Models;

namespace PizzaShopServices.Interfaces;

public interface IModifierService
{
    
    Task<List<Modifier>> GetAllModifiersAsync();
}