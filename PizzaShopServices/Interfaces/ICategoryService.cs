using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<(bool Success, string Message)> AddCategoryAsync(CrudCategoryViewModel model);
    Task<CrudCategoryViewModel?> GetCategoryForEditAsync(int id);
    Task<(bool Success, string Message)> UpdateCategoryAsync(CrudCategoryViewModel model);
    Task SoftDeleteCategoryAsync(int id);
    // Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync();
    // Task<CategoryViewModel> GetCategoryByIdAsync(int id);
    // Task<Category> CreateCategoryAsync(CreateCategoryVM model);
    // Task<Category> UpdateCategoryAsync(UpdateCategoryVM model);
    // Task SoftDeleteCategoryAsync(int id);
}

