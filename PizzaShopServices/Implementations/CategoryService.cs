using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;

namespace PizzaShopServices.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllCategoriesAsync();
        return categories.Select(c => new CategoryViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsDeleted = c.IsDeleted
        });
    }

    public async Task<CategoryViewModel> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(id);
        if (category == null) return null;
        return new CategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsDeleted = category.IsDeleted
        };
    }

    public async Task<Category> CreateCategoryAsync(CreateCategoryVM model)
        {
            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                IsDeleted = false
            };
            return await _categoryRepository.AddCategoryAsync(category);
        }

        public async Task<Category> UpdateCategoryAsync(UpdateCategoryVM model)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(model.Id);
            if (category == null)
                throw new Exception("Category not found");

            category.Name = model.Name;
            category.Description = model.Description;
            return await _categoryRepository.UpdateCategoryAsync(category);
        }

        public async Task SoftDeleteCategoryAsync(int id)
        {
            await _categoryRepository.SoftDeleteCategoryAsync(id);
        }
}
