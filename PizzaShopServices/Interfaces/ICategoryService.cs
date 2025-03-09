using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync();
    Task<CategoryViewModel> GetCategoryByIdAsync(int id);
}
