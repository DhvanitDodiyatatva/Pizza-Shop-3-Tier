using Microsoft.AspNetCore.Http;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface IItemService
{
    Task<List<Item>> GetItemsByCategoryAsync(int categoryId);
    Task<List<Item>> GetAllItemsAsync();
    Task<List<Item>> GetFavoriteItemsAsync();
    Task<(bool Success, string Message)> ToggleFavoriteAsync(int itemId);
    Task<(bool Success, string Message)> AddItemAsync(ItemVM model);
    Task<ItemVM?> GetItemForEditAsync(int id);
    Task<(bool Success, string Message)> UpdateItemAsync(ItemVM model, IFormFile ImageFile, string host);
    Task<Item?> GetItemByIdAsync(int id);
    Task SoftDeleteItemAsync(int id);
    Task SoftDeleteItemsAsync(List<int> ids);

}
