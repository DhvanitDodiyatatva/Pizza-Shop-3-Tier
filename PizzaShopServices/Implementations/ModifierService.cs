using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.Repositories;
using PizzaShopServices.Interfaces;

namespace PizzaShopServices.Implementations;

public class ModifierService : IModifierService
{
    private readonly IModifierRepository _modifierRepository;
    private readonly IModifierGroupMappingRepository _modifierGroupMappingRepository;
    public ModifierService(IModifierRepository modifierRepository, IModifierGroupMappingRepository modifierGroupMappingRepository)
    {
        _modifierRepository = modifierRepository;
        _modifierGroupMappingRepository = modifierGroupMappingRepository;
    }
    public async Task<List<Modifier>> GetAllModifiersAsync()
    {
        return await _modifierRepository.GetAllModifierAsync();
    }



}
