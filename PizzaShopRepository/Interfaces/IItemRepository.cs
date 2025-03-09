using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface IItemRepository
{
    Task<IEnumerable<Item>> GetAllItemsAsync();
    Task<IEnumerable<Item>> GetItemsByCategoryIdAsync(int categoryId);
    Task<Item> GetItemByIdAsync(int id);
}
