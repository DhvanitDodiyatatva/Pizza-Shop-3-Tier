using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopServices.Implementations
{
    public class KotService : IKotService
    {
        private readonly IKotRepository _kotRepository;
        private readonly ICategoryService _categoryService;

        public KotService(IKotRepository kotRepository, ICategoryService categoryService)
        {
            _kotRepository = kotRepository;
            _categoryService = categoryService;
        }

        public async Task<List<Order>> GetOrdersByCategoryAndStatusAsync(string categoryName, string status)
        {
            int? categoryId = null;
            if (!string.IsNullOrEmpty(categoryName) && categoryName != "All")
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                var category = categories.FirstOrDefault(c => c.Name == categoryName);
                categoryId = category?.Id;
            }

            return await _kotRepository.GetOrdersByCategoryAndStatusAsync(categoryId, status);
        }
    }
}
