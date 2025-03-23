using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.Repositories;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;

namespace PizzaShopServices.Implementations;

public class ModifierGroupService : IModifierGroupService
{
    private readonly IModifierGroupRepository _modifierGrpRepository;
    private readonly IModifierGroupMappingRepository _modifierGroupMappingRepository;
    private readonly IModifierService _modifierService;

    public ModifierGroupService(IModifierGroupRepository modifierGroupRepository, IModifierGroupMappingRepository modifierGroupMappingRepository, IModifierService modifierService)
    {
        _modifierGrpRepository = modifierGroupRepository;
        _modifierGroupMappingRepository = modifierGroupMappingRepository;
        _modifierService = modifierService;
    }

    public async Task<List<ModifierGroup>> GetAllModifierGroupAsync()
    {
        return await _modifierGrpRepository.GetAllModifierGroupsAsync();
    }

    public async Task<ModifierGroupViewModel> AddModifierGroupAsync(ModifierGroupViewModel model)
    {
        // Check if a modifier group with the same name already exists (case-insensitive)
        var existingModifierGroup = (await _modifierGrpRepository.GetAllModifierGroupsAsync())
            .FirstOrDefault(mg => mg.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase));

        if (existingModifierGroup != null)
        {
            throw new InvalidOperationException("This modifier group name already exists, choose a different name.");
        }

        var modifierGroup = new ModifierGroup
        {
            Name = model.Name,
            Description = model.Description,
            IsDeleted = false
        };

        await _modifierGrpRepository.AddModifierGroupAsync(modifierGroup);

        // Add mappings for selected modifiers
        if (model.SelectedModifierIds != null && model.SelectedModifierIds.Any())
        {
            foreach (var modifierId in model.SelectedModifierIds)
            {
                await _modifierGroupMappingRepository.CreateMappingAsync(modifierGroup.Id, modifierId);
            }
        }

        model.Id = modifierGroup.Id;
        return model;
    }

    public async Task<ModifierGroupViewModel?> GetModifierGroupForEditAsync(int id)
    {
        var modifierGroup = await _modifierGrpRepository.GetModifierGroupByIdAsync(id);
        if (modifierGroup == null) return null;

        var mappings = await _modifierGroupMappingRepository.GetModifiersByModifierGroupIdAsync(id);
        var selectedModifierIds = mappings.Select(m => m.Id).ToList();
        var selectedModifiers = mappings;

        return new ModifierGroupViewModel
        {
            Id = modifierGroup.Id,
            Name = modifierGroup.Name,
            Description = modifierGroup.Description,
            SelectedModifierIds = selectedModifierIds,
            SelectedModifiers = selectedModifiers // Populate the new property
        };
    }

    public async Task<ModifierGroupViewModel> UpdateModifierGroupAsync(ModifierGroupViewModel model)
    {
        var modifierGroup = await _modifierGrpRepository.GetModifierGroupByIdAsync(model.Id);
        if (modifierGroup == null) throw new Exception("Modifier Group not found");

        // Check if another modifier group with the same name exists (case-insensitive), excluding the current one
        var existingModifierGroup = (await _modifierGrpRepository.GetAllModifierGroupsAsync())
            .FirstOrDefault(mg => mg.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) && mg.Id != model.Id);

        if (existingModifierGroup != null)
        {
            throw new InvalidOperationException("This modifier group name already exists, choose a different name.");
        }

        modifierGroup.Name = model.Name;
        modifierGroup.Description = model.Description;

        await _modifierGrpRepository.UpdateModifierGroupAsync(modifierGroup);

        // Get existing mappings (including those marked as deleted to avoid losing them)
        var existingMappings = await _modifierGroupMappingRepository.GetMappingsForModifierGroupAsync(model.Id);
        var existingModifierIds = existingMappings.Select(m => m.ModifierId).ToList();

        // If SelectedModifierIds is null or empty, mark all existing mappings as deleted
        if (model.SelectedModifierIds == null || !model.SelectedModifierIds.Any())
        {
            foreach (var mapping in existingMappings)
            {
                if (!mapping.IsDeleted) // Only update if not already deleted
                {
                    mapping.IsDeleted = true;
                    await _modifierGroupMappingRepository.UpdateMappingAsync(mapping);
                }
            }
        }
        else
        {
            // Mark mappings as deleted if their ModifierId is not in SelectedModifierIds
            foreach (var mapping in existingMappings)
            {
                if (!model.SelectedModifierIds.Contains(mapping.ModifierId))
                {
                    if (!mapping.IsDeleted) // Only update if not already deleted
                    {
                        mapping.IsDeleted = true;
                        await _modifierGroupMappingRepository.UpdateMappingAsync(mapping);
                    }
                }
                else
                {
                    // If the mapping was previously deleted but is now re-added, restore it
                    if (mapping.IsDeleted)
                    {
                        mapping.IsDeleted = false;
                        await _modifierGroupMappingRepository.UpdateMappingAsync(mapping);
                    }
                }
            }

            // Add new mappings for any ModifierId in SelectedModifierIds that doesn't exist in existingMappings
            foreach (var modifierId in model.SelectedModifierIds)
            {
                if (!existingModifierIds.Contains(modifierId))
                {
                    await _modifierGroupMappingRepository.CreateMappingAsync(model.Id, modifierId);
                }
            }
        }

        return model;
    }


    public async Task DeleteModifierGroupAsync(int id)
    {
        var modifierGroup = await _modifierGrpRepository.GetModifierGroupByIdAsync(id);
        if (modifierGroup == null)
        {
            throw new Exception("Modifier Group not found.");
        }

        // Soft delete the modifier group
        await _modifierGrpRepository.SoftDeleteModifierGroupAsync(id);

        // Optionally, soft delete associated mappings
        var mappings = await _modifierGroupMappingRepository.GetMappingsForModifierGroupAsync(id);
        foreach (var mapping in mappings)
        {
            if (!mapping.IsDeleted)
            {
                mapping.IsDeleted = true;
                await _modifierGroupMappingRepository.UpdateMappingAsync(mapping);
            }
        }
    }
}
