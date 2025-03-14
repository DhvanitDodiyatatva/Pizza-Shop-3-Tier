using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface IItemService
{
    Task<List<Item>> GetItemsByCategoryAsync(int categoryId);
    Task<List<Item>> GetAllItemsAsync();
    Task<(bool Success, string Message)> AddItemAsync(ItemVM model);
    Task<ItemVM?> GetItemForEditAsync(int id);
    Task<(bool Success, string Message)> UpdateItemAsync(ItemVM model);
    Task<Item?> GetItemByIdAsync(int id);
    Task SoftDeleteItemAsync(int id);
}
