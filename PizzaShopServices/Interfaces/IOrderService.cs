// PizzaShopService/Interfaces/IOrderService.cs
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopService.Interfaces;

public interface IOrderService
{
    Task<OrderViewModel> GetOrdersAsync(string searchQuery, string statusFilter, string timeFilter, string fromDate, string toDate, string sortColumn, string sortDirection, int page, int pageSize);
}