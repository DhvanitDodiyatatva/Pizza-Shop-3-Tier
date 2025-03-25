// PizzaShopRepository/Interfaces/IOrderRepository.cs
using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface IOrderRepository
{
    IQueryable<Order> GetOrders();
    Task<List<Order>> GetPaginatedOrdersAsync(string searchQuery, string statusFilter, string timeFilter, string fromDate, string toDate, string sortColumn, string sortDirection, int page, int pageSize);
    Task<int> GetTotalOrdersCountAsync(string searchQuery, string statusFilter, string timeFilter, string fromDate, string toDate);
}