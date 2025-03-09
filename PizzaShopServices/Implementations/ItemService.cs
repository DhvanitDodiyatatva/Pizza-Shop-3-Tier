using PizzaShopRepository.Interfaces;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;

namespace PizzaShopServices.Implementations;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;

    public ItemService(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<IEnumerable<ItemVMViewModel>> GetAllItemsAsync()
    {
        var items = await _itemRepository.GetAllItemsAsync();
        return items.Select(i => new ItemVMViewModel
        {
            Id = i.Id,
            CategoryId = i.CategoryId,
            Name = i.Name,
            Description = i.Description,
            Price = i.Price,
            ItemType = i.ItemType,
            Quantity = i.Quantity,
            Unit = i.Unit,
            IsAvailable = i.IsAvailable,
            ShortCode = i.ShortCode,
            ImageUrl = i.ImageUrl,
            IsDeleted = i.IsDeleted,
            CategoryName = i.Category?.Name
        });
    }

    public async Task<IEnumerable<ItemVMViewModel>> GetItemsByCategoryIdAsync(int categoryId)
    {
        var items = await _itemRepository.GetItemsByCategoryIdAsync(categoryId);
        return items.Select(i => new ItemVMViewModel
        {
            Id = i.Id,
            CategoryId = i.CategoryId,
            Name = i.Name,
            Description = i.Description,
            Price = i.Price,
            ItemType = i.ItemType,
            Quantity = i.Quantity,
            Unit = i.Unit,
            IsAvailable = i.IsAvailable,
            ShortCode = i.ShortCode,
            ImageUrl = i.ImageUrl,
            IsDeleted = i.IsDeleted,
            CategoryName = i.Category?.Name
        });
    }

    public async Task<ItemVMViewModel> GetItemByIdAsync(int id)
    {
        var item = await _itemRepository.GetItemByIdAsync(id);
        if (item == null) return null;
        return new ItemVMViewModel
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
            IsDeleted = item.IsDeleted,
            CategoryName = item.Category?.Name
        };
    }
}
