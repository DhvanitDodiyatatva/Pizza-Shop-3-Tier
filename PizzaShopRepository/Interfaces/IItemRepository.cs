using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface IItemRepository
{
    Task<List<Item>> GetAllItemsAsync();
    Task<List<Item>> GetItemsByCategoryAsync(int categoryId);

}
