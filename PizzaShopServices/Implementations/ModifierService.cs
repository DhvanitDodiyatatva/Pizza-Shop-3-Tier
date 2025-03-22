using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.Repositories;
using PizzaShopRepository.ViewModels;
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

    public async Task<ModifierViewModel> AddModifierAsync(ModifierViewModel model)
    {
        if (model.ModifierGroupIds == null || !model.ModifierGroupIds.Any())
        {
            throw new ArgumentException("At least one Modifier Group must be selected.");
        }

        var modifier = new Modifier
        {
            Name = model.Name,
            Price = model.Price,
            Unit = model.Unit,
            Quantity = model.Quantity ?? 0, // Ensure Quantity has a default if null
            Description = model.Description,
            IsDeleted = false
        };

        await _modifierRepository.AddModifierAsync(modifier);

        foreach (var groupId in model.ModifierGroupIds)
        {
            // Verify ModifierGroupId exists (optional, depending on your requirements)
            await _modifierGroupMappingRepository.CreateMappingAsync(groupId, modifier.Id);
        }

        model.Id = modifier.Id;
        return model;
    }

    public async Task<ModifierViewModel?> GetModifierForEditAsync(int id)
    {
        var modifier = await _modifierRepository.GetModifierByIdAsync(id);
        if (modifier == null) return null;

        var mappings = await _modifierGroupMappingRepository.GetMappingsForModifierAsync(id);
        var modifierGroupIds = mappings.Select(m => m.ModifierGroupId).ToList();

        return new ModifierViewModel
        {
            Id = modifier.Id,
            Name = modifier.Name,
            Price = modifier.Price,
            Unit = modifier.Unit,
            Quantity = modifier.Quantity,
            Description = modifier.Description,
            ModifierGroupIds = modifierGroupIds
        };
    }

    public async Task<ModifierViewModel> UpdateModifierAsync(ModifierViewModel model)
    {
        var modifier = await _modifierRepository.GetModifierByIdAsync(model.Id);
        if (modifier == null) throw new Exception("Modifier not found");

        modifier.Name = model.Name;
        modifier.Price = model.Price;
        modifier.Unit = model.Unit;
        modifier.Quantity = model.Quantity ?? 0; // Ensure Quantity has a default if null
        modifier.Description = model.Description;

        await _modifierRepository.UpdateModifierAsync(modifier);

        var existingMappings = await _modifierGroupMappingRepository.GetMappingsForModifierAsync(model.Id);
        var existingGroupIds = existingMappings.Select(m => m.ModifierGroupId).ToList();

        foreach (var mapping in existingMappings)
        {
            if (!model.ModifierGroupIds.Contains(mapping.ModifierGroupId))
            {
                mapping.IsDeleted = true;
                await _modifierGroupMappingRepository.UpdateMappingAsync(mapping); // Add await
            }
        }

        foreach (var groupId in model.ModifierGroupIds)
        {
            if (!existingGroupIds.Contains(groupId))
            {
                await _modifierGroupMappingRepository.CreateMappingAsync(groupId, model.Id);
            }
        }

        return model;
    }

    // Soft Delete a Modifier from a Specific Group
    public async Task SoftDeleteModifierFromGroupAsync(int modifierId, int modifierGroupId)
    {
        var modifier = await _modifierRepository.GetModifierByIdAsync(modifierId);
        if (modifier == null) throw new Exception("Modifier not found");

        var mapping = (await _modifierGroupMappingRepository.GetMappingsForModifierAsync(modifierId))
            .FirstOrDefault(m => m.ModifierGroupId == modifierGroupId && !m.IsDeleted);

        if (mapping != null)
        {
            mapping.IsDeleted = true;
            await _modifierGroupMappingRepository.UpdateMappingAsync(mapping);

            // Check if the modifier is no longer mapped to any group
            var remainingMappings = await _modifierGroupMappingRepository.GetMappingsForModifierAsync(modifierId);
            if (!remainingMappings.Any(m => !m.IsDeleted))
            {
                modifier.IsDeleted = true;
                await _modifierRepository.UpdateModifierAsync(modifier);
            }
        }
    }

    //Mass Soft Delete Modifiers from a Specific Group
    public async Task SoftDeleteModifiersFromGroupAsync(List<int> modifierIds, int modifierGroupId)
    {
        foreach (var id in modifierIds)
        {
            var modifier = await _modifierRepository.GetModifierByIdAsync(id);
            if (modifier != null && !modifier.IsDeleted)
            {
                var mapping = (await _modifierGroupMappingRepository.GetMappingsForModifierAsync(id))
                    .FirstOrDefault(m => m.ModifierGroupId == modifierGroupId && !m.IsDeleted);

                if (mapping != null)
                {
                    mapping.IsDeleted = true;
                    await _modifierGroupMappingRepository.UpdateMappingAsync(mapping);

                    // Check if the modifier is no longer mapped to any group
                    var remainingMappings = await _modifierGroupMappingRepository.GetMappingsForModifierAsync(id);
                    if (!remainingMappings.Any(m => !m.IsDeleted))
                    {
                        modifier.IsDeleted = true;
                        await _modifierRepository.UpdateModifierAsync(modifier);
                    }
                }
            }
        }
    }


}
