using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface IModifierRepository
{
    Task<List<Modifier>> GetAllModifierAsync();
    Task<Modifier?> GetModifierByIdAsync(int id);
    Task AddModifierAsync(Modifier modifier);

}
