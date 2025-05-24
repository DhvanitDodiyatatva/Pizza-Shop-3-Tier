using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopRepository.Interfaces
{
    public interface IKotRepository
    {
        // Task<List<Order>> GetOrdersByCategoryAndStatusAsync(int? categoryId, string status);
        Task<List<KotOrderViewModel>> GetOrdersByCategoryAndStatusAsync(int? categoryId, string status);
        Task<bool> UpdateOrderItemStatusesAsync(int orderId, List<(int OrderItemId, int AdjustedQuantity)> items, string newStatus);
    }
}