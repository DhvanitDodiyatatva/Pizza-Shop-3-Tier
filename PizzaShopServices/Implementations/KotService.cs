using Microsoft.AspNetCore.SignalR;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using PizzaShopService.Hubs;



namespace PizzaShopServices.Implementations
{
    public class KotService : IKotService
    {
        private readonly IKotRepository _kotRepository;
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<KotHub> _hubContext;

        public KotService(IKotRepository kotRepository, ICategoryService categoryService, IHubContext<KotHub> hubContext)
        {
            _kotRepository = kotRepository;
            _categoryService = categoryService;
            _hubContext = hubContext;
        }

        // public async Task<List<Order>> GetOrdersByCategoryAndStatusAsync(string categoryName, string status)
        // {
        //     int? categoryId = null;
        //     if (!string.IsNullOrEmpty(categoryName) && categoryName != "All")
        //     {
        //         var categories = await _categoryService.GetAllCategoriesAsync();
        //         var category = categories.FirstOrDefault(c => c.Name == categoryName);
        //         categoryId = category?.Id;
        //     }

        //     return await _kotRepository.GetOrdersByCategoryAndStatusAsync(categoryId, status);
        // }

        public async Task<List<KotOrderViewModel>> GetOrdersByCategoryAndStatusAsync(string categoryName, string status)
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

        public async Task<bool> UpdateOrderItemStatusesAsync(int orderId, List<(int OrderItemId, int AdjustedQuantity)> items, string newStatus)
        {
            if (items == null || !items.Any())
            {
                return false;
            }

            var result = await _kotRepository.UpdateOrderItemStatusesAsync(orderId, items, newStatus);
            if (result)
            {
                // Broadcast the update to all clients
                await _hubContext.Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, newStatus);
            }
            return result;
        }
    }
}
