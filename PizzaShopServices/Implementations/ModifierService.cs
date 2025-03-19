using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopServices.Interfaces;

namespace PizzaShopServices.Implementations;

public class ModifierService : IModifierService
{
    private readonly IModifierRepository _modifierRepository;
    public ModifierService(IModifierRepository modifierRepository)
    {
        _modifierRepository = modifierRepository;
    }
    public async Task<List<Modifier>> GetAllModifiersAsync()
    {
        return await _modifierRepository.GetAllModifiersAsync();
    }

    public async Task<Modifier?> GetModifierByIdAsync(int id)
    {
        return await _modifierRepository.GetModifierByIdAsync(id);
    }

    // public async Task<List<Modifier>> GetModifiersByModifierGrpAsync(int modifierGroupId)
    // {
    //     return await _modifierRepository.GetModifiersByModifierGrpAsync(modifierGroupId);
    // }

}
