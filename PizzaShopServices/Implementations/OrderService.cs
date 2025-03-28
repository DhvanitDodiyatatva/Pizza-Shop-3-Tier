using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopService.Interfaces;

namespace PizzaShopService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderViewModel> GetOrdersAsync(string searchQuery, string statusFilter, string timeFilter, string fromDate, string toDate, string sortColumn, string sortDirection, int page, int pageSize)
    {
        var orders = await _orderRepository.GetPaginatedOrdersAsync(searchQuery, statusFilter, timeFilter, fromDate, toDate, sortColumn, sortDirection, page, pageSize);
        var totalOrders = await _orderRepository.GetTotalOrdersCountAsync(searchQuery, statusFilter, timeFilter, fromDate, toDate);

        return new OrderViewModel
        {
            Orders = orders,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize),
            PageSize = pageSize,
            SearchQuery = searchQuery,
            SortColumn = sortColumn,
            SortDirection = sortDirection
        };
    }

    public async Task<Order?> GetOrderDetailsAsync(int id)
    {
        return await _orderRepository.GetOrderByIdAsync(id);
    }
}