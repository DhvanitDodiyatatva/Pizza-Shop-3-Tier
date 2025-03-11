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

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _categoryRepository.GetAllCategoriesAsync();
    }

    public async Task<(bool Success, string Message)> AddCategoryAsync(CrudCategoryViewModel model)
    {
        var existingCategory = await _categoryRepository.GetCategory(model.Name);
        if (existingCategory != null)
        {
            return (false, "Category already exists.");
        }

        var category = new Category
        {
            Name = model.Name,
            Description = model.Description
        };

        try
        {
            await _categoryRepository.AddCategoriesAsync(category);
            return (true, "Category added successfully.");
        }
        catch (Exception ex)
        {
            return (false, "Failed to add Category: " + ex.Message);
        }
    }

    public async Task<CrudCategoryViewModel?> GetCategoryForEditAsync(int id)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(id)
;
        if (category == null)
            return null;

        return new CrudCategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<(bool Success, string Message)> UpdateCategoryAsync(CrudCategoryViewModel model)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(model.Id);
        if (category == null)
        {
            return (false, "Category not found.");
        }

        // Check if the category name is already taken by another category.
        var existingCategory = await _categoryRepository.GetCategory(model.Name);
        if (existingCategory != null && existingCategory.Id != model.Id)
        {
            return (false, "Category name already exists. Please choose another name.");
        }

        // Update properties from the model
        category.Name = model.Name;
        category.Description = model.Description;


        try
        {
            await _categoryRepository.UpdateCategoriesAsync(category);
            return (true, "Category updated successfully!");
        }
        catch (Exception ex)
        {
            return (false, "Failed to update Category: " + ex.Message);
        }
    }

    public async Task SoftDeleteCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(id)
;
        if (category == null)
        {
            return;
        }

        category.IsDeleted = true;
        await _categoryRepository.UpdateCategoriesAsync(category);
    }

    // public async Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync()
    // {
    //     var categories = await _categoryRepository.GetAllCategoriesAsync();
    //     return categories.Select(c => new CategoryViewModel
    //     {
    //         Id = c.Id,
    //         Name = c.Name,
    //         Description = c.Description,
    //         IsDeleted = c.IsDeleted
    //     });
    // }

    // public async Task<CategoryViewModel> GetCategoryByIdAsync(int id)
    // {
    //     var category = await _categoryRepository.GetCategoryByIdAsync(id);
    //     if (category == null) return null;
    //     return new CategoryViewModel
    //     {
    //         Id = category.Id,
    //         Name = category.Name,
    //         Description = category.Description,
    //         IsDeleted = category.IsDeleted
    //     };
    // }

    // public async Task<Category> CreateCategoryAsync(CreateCategoryVM model)
    //     {
    //         var category = new Category
    //         {
    //             Name = model.Name,
    //             Description = model.Description,
    //             IsDeleted = false
    //         };
    //         return await _categoryRepository.AddCategoryAsync(category);
    //     }

    //     public async Task<Category> UpdateCategoryAsync(UpdateCategoryVM model)
    //     {
    //         var category = await _categoryRepository.GetCategoryByIdAsync(model.Id);
    //         if (category == null)
    //             throw new Exception("Category not found");

    //         category.Name = model.Name;
    //         category.Description = model.Description;
    //         return await _categoryRepository.UpdateCategoryAsync(category);
    //     }

    //     public async Task SoftDeleteCategoryAsync(int id)
    //     {
    //         await _categoryRepository.SoftDeleteCategoryAsync(id);
    //     }
}
