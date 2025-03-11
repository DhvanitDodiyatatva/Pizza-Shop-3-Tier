using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface IItemService
{
    Task<List<Item>> GetItemsByCategoryAsync(int categoryId);
    Task<List<Item>> GetAllItemsAsync();

}
