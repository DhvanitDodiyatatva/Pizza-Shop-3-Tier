using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
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

    public async Task<List<Item>> GetItemsByCategoryAsync(int categoryId)
    {
        return await _itemRepository.GetItemsByCategoryAsync(categoryId);
    }

    public async Task<List<Item>> GetAllItemsAsync()
    {
        return await _itemRepository.GetAllItemsAsync();
    }
}
