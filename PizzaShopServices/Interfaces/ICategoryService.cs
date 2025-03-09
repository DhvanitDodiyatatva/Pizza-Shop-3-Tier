using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync();
    Task<CategoryViewModel> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(CreateCategoryVM model);
    Task<Category> UpdateCategoryAsync(UpdateCategoryVM model);
    Task SoftDeleteCategoryAsync(int id);
}

