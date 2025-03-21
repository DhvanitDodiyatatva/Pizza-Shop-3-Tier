using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopServices.Interfaces;

namespace PizzaShopServices.Implementations;

public class ModifierGroupService : IModifierGroupService
{
    private readonly IModifierGroupRepository _modifierGrpRepository;

    public ModifierGroupService(IModifierGroupRepository modifierGroupRepository)
    {
        _modifierGrpRepository = modifierGroupRepository;
    }
    public async Task<List<ModifierGroup>> GetAllModifierGroupAsync()
    {
        return await _modifierGrpRepository.GetAllModifierGroupsAsync();
    }
}
