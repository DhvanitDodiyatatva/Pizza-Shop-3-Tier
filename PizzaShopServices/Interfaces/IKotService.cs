using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopServices.Interfaces
{
    public interface IKotService
    {
        // Task<List<Order>> GetOrdersByCategoryAndStatusAsync(string categoryName, string status);
        Task<List<KotOrderViewModel>> GetOrdersByCategoryAndStatusAsync(string categoryName, string status);

        Task<bool> UpdateOrderItemStatusesAsync(int orderId, List<(int OrderItemId, int AdjustedQuantity)> items, string newStatus);
    }
}