using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface IItemService
{
    Task<IEnumerable<ItemVMViewModel>> GetAllItemsAsync();
    Task<IEnumerable<ItemVMViewModel>> GetItemsByCategoryIdAsync(int categoryId);
    Task<ItemVMViewModel> GetItemByIdAsync(int id);
}
