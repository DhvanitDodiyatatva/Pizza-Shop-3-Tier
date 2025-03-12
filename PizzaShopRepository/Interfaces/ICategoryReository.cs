using PizzaShopRepository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopRepository.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task UpdateCategoriesAsync(Category category);
    Task AddCategoriesAsync(Category category);
    Task<Category?> GetCategory(string name);
}