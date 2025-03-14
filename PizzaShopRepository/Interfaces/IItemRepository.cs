using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface IItemRepository
{
    Task<List<Item>> GetItemsByCategoryAsync(int categoryId);
    Task<List<Item>> GetAllItemsAsync();
    Task<Item?> GetItemByIdAsync(int id);
    Task AddItemAsync(Item item);
    Task UpdateItemAsync(Item item);
    Task<Item?> GetItemByNameAsync(string name);
    Task SoftDeleteItemsAsync(List<int> ids);    


}
