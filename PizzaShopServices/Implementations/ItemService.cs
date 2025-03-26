using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using PizzaShopRepository.Data;

namespace PizzaShopServices.Implementations;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;
    private readonly IItemModifierGroupService _itemModifierGroupService;


    public ItemService(IItemRepository itemRepository, IItemModifierGroupService itemModifierGroupService)
    {
        _itemRepository = itemRepository;
        _itemModifierGroupService = itemModifierGroupService;
    }

    public async Task<List<Item>> GetItemsByCategoryAsync(int categoryId)
    {
        return await _itemRepository.GetItemsByCategoryAsync(categoryId);
    }

    public async Task<List<Item>> GetAllItemsAsync()
    {
        return await _itemRepository.GetAllItemsAsync();
    }

    public async Task<(bool Success, string Message)> AddItemAsync(ItemVM model)
    {
        // Check if an item with the same name already exists
        var existingItem = await _itemRepository.GetItemByNameAsync(model.Name);
        if (existingItem != null)
        {
            return (false, "Item with this name already exists!");
        }

        var item = new Item
        {
            CategoryId = model.CategoryId,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            ItemType = model.ItemType,
            Quantity = model.Quantity,
            Unit = model.Unit,
            IsAvailable = model.IsAvailable,
            ShortCode = model.ShortCode,
            ImageUrl = model.ImageUrl,
            TaxPercentage = model.TaxPercentage,
            DefaultTax = model.DefaultTax,
            CreatedAt = DateTime.Now,
            IsDeleted = false
        };

        try
        {
            await _itemRepository.AddItemAsync(item);

            // Add ItemModifierGroup mappings
            if (model.SelectedModifierGroupIds != null && model.SelectedModifierGroupIds.Any())
            {
                foreach (var modifierGroupId in model.SelectedModifierGroupIds)
                {
                    var config = model.ModifierGroupConfigs?.FirstOrDefault(c => c.ModifierGroupId == modifierGroupId);
                    await _itemModifierGroupService.AddItemModifierGroupAsync(
                        item.Id,
                        modifierGroupId,
                        config?.MinLoad,
                        config?.MaxLoad
                    );
                }
            }

            return (true, "Item added successfully!");
        }
        catch (Exception ex)
        {
            var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return (false, $"Failed to add item: {innerException}");
        }
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        return await _itemRepository.GetItemByIdAsync(id);
    }


    public async Task<ItemVM?> GetItemForEditAsync(int id)
    {
        var item = await _itemRepository.GetItemByIdAsync(id);
        if (item == null)
            return null;

        var itemModifierGroups = await _itemModifierGroupService.GetItemModifierGroupsByItemIdAsync(id);

        return new ItemVM
        {
            Id = item.Id,
            CategoryId = item.CategoryId,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            ItemType = item.ItemType,
            Quantity = item.Quantity,
            Unit = item.Unit,
            IsAvailable = item.IsAvailable,
            ShortCode = item.ShortCode,
            ImageUrl = item.ImageUrl,
            TaxPercentage = item.TaxPercentage,
            DefaultTax = item.DefaultTax,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            SelectedModifierGroupIds = itemModifierGroups.Select(img => img.ModifierGroupId).ToList(),
            ModifierGroupConfigs = itemModifierGroups.Select(img => new ModifierGroupConfig
            {
                ModifierGroupId = img.ModifierGroupId,
                MinLoad = img.MinLoad,
                MaxLoad = img.MaxLoad
            }).ToList()
        };
    }

    public async Task<(bool Success, string Message)> UpdateItemAsync(ItemVM model)
    {
        // Check if an item with the same name already exists
        var existingItem = await _itemRepository.GetItemByNameAsync(model.Name);
        if (existingItem != null && existingItem.Id != model.Id)
        {
            return (false, "Item with this name already exists. Please choose a different name.");
        }



        var item = await _itemRepository.GetItemByIdAsync(model.Id);
        if (item == null)
        {
            return (false, "Item not found.");
        }


        item.CategoryId = model.CategoryId;
        item.Name = model.Name;
        item.Description = model.Description;
        item.Price = model.Price;
        item.ItemType = model.ItemType;
        item.Quantity = model.Quantity;
        item.Unit = model.Unit;
        item.IsAvailable = model.IsAvailable;
        item.ShortCode = model.ShortCode;
        item.ImageUrl = model.ImageUrl;
        item.TaxPercentage = model.TaxPercentage;
        item.DefaultTax = model.DefaultTax;
        item.UpdatedAt = DateTime.Now;

        try
        {
            await _itemRepository.UpdateItemAsync(item);

            // Update ItemModifierGroup mappings
            var existingMappings = await _itemModifierGroupService.GetItemModifierGroupsByItemIdAsync(model.Id);
            var existingModifierGroupIds = existingMappings.Select(img => img.ModifierGroupId).ToList();

            // Remove mappings that are no longer selected
            foreach (var existingMapping in existingMappings)
            {
                if (!model.SelectedModifierGroupIds.Contains(existingMapping.ModifierGroupId))
                {
                    await _itemModifierGroupService.DeleteItemModifierGroupAsync(model.Id, existingMapping.ModifierGroupId);
                }
            }

            // Add or update mappings for selected modifier groups
            foreach (var modifierGroupId in model.SelectedModifierGroupIds)
            {
                var config = model.ModifierGroupConfigs?.FirstOrDefault(c => c.ModifierGroupId == modifierGroupId);
                if (existingModifierGroupIds.Contains(modifierGroupId))
                {
                    // Update existing mapping
                    await _itemModifierGroupService.UpdateItemModifierGroupAsync(
                        model.Id,
                        modifierGroupId,
                        config?.MinLoad,
                        config?.MaxLoad
                    );
                }
                else
                {
                    // Add new mapping
                    await _itemModifierGroupService.AddItemModifierGroupAsync(
                        model.Id,
                        modifierGroupId,
                        config?.MinLoad,
                        config?.MaxLoad
                    );
                }
            }

            return (true, "Item updated successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in UpdateItemAsync: {ex.Message}"); // Debugging
            return (false, $"Failed to update item: {ex.Message}");
        }
    }

    public async Task SoftDeleteItemAsync(int id)
    {
        var item = await _itemRepository.GetItemByIdAsync(id);
        if (item == null)
        {
            return;
        }

        item.IsDeleted = true;
        await _itemRepository.UpdateItemAsync(item);
        
        // Soft delete associated ItemModifierGroup mappings
        var itemModifierGroups = await _itemModifierGroupService.GetItemModifierGroupsByItemIdAsync(id);
        foreach (var img in itemModifierGroups)
        {
            await _itemModifierGroupService.DeleteItemModifierGroupAsync(id, img.ModifierGroupId);
        }
    }

    public async Task SoftDeleteItemsAsync(List<int> ids)
    {
        await _itemRepository.SoftDeleteItemsAsync(ids);
    }



}
