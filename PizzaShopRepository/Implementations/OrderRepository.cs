// PizzaShopRepository/Repositories/OrderRepository.cs
using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;

namespace PizzaShopRepository.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly PizzaShopContext _context;

    public OrderRepository(PizzaShopContext context)
    {
        _context = context;
    }

    public IQueryable<Order> GetOrders()
    {
        return _context.Orders.Include(o => o.Customer);
    }

    public async Task<List<Order>> GetPaginatedOrdersAsync(string searchQuery, string statusFilter, string timeFilter, string fromDate, string toDate, string sortColumn, string sortDirection, int page, int pageSize)
    {
        var query = GetOrders();

        // Search
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(o => o.Customer.Name.Contains(searchQuery) ||
                                   o.Id.ToString().Contains(searchQuery) ||
                                   o.OrderStatus.Contains(searchQuery));
        }

        // Status Filter
        if (!string.IsNullOrEmpty(statusFilter))
        {
            query = query.Where(o => o.OrderStatus.ToLower() == statusFilter.ToLower());
        }

        // Time Filter
        if (!string.IsNullOrEmpty(timeFilter))
        {
            var today = DateTime.Today;
            switch (timeFilter.ToLower())
            {
                case "today":
                    query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == today);
                    break;
                case "this_week":
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= startOfWeek && o.CreatedAt.Value.Date <= today);
                    break;
                case "this_month":
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= startOfMonth && o.CreatedAt.Value.Date <= today);
                    break;
            }
        }

        // Date Range Filter
        if (!string.IsNullOrEmpty(fromDate))
        {
            if (DateTime.TryParse(fromDate, out var fromDateTime))
            {
                query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= fromDateTime.Date);
            }
        }
        if (!string.IsNullOrEmpty(toDate))
        {
            if (DateTime.TryParse(toDate, out var toDateTime))
            {
                query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date <= toDateTime.Date);
            }
        }

        // Sorting
        switch (sortColumn?.ToLower())
        {
            case "order":
                query = sortDirection == "asc" ? query.OrderBy(o => o.Id) : query.OrderByDescending(o => o.Id);
                break;
            case "date":
                query = sortDirection == "asc" ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt);
                break;
            case "customer":
                query = sortDirection == "asc" ? query.OrderBy(o => o.Customer.Name) : query.OrderByDescending(o => o.Customer.Name);
                break;
            case "total amount":
                query = sortDirection == "asc" ? query.OrderBy(o => o.TotalAmount) : query.OrderByDescending(o => o.TotalAmount);
                break;
            default:
                query = query.OrderByDescending(o => o.CreatedAt);
                break;
        }

        // Pagination
        return await query.Skip((page - 1) * pageSize)
                         .Take(pageSize)
                         .ToListAsync();
    }

    public async Task<int> GetTotalOrdersCountAsync(string searchQuery, string statusFilter, string timeFilter, string fromDate, string toDate)
    {
        var query = GetOrders();

        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(o => o.Customer.Name.Contains(searchQuery) ||
                                   o.Id.ToString().Contains(searchQuery) ||
                                   o.OrderStatus.Contains(searchQuery));
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            query = query.Where(o => o.OrderStatus.ToLower() == statusFilter.ToLower());
        }

        if (!string.IsNullOrEmpty(timeFilter))
        {
            var today = DateTime.Today;
            switch (timeFilter.ToLower())
            {
                case "today":
                    query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date == today);
                    break;
                case "this_week":
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= startOfWeek && o.CreatedAt.Value.Date <= today);
                    break;
                case "this_month":
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= startOfMonth && o.CreatedAt.Value.Date <= today);
                    break;
            }
        }

        if (!string.IsNullOrEmpty(fromDate))
        {
            if (DateTime.TryParse(fromDate, out var fromDateTime))
            {
                query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date >= fromDateTime.Date);
            }
        }
        if (!string.IsNullOrEmpty(toDate))
        {
            if (DateTime.TryParse(toDate, out var toDateTime))
            {
                query = query.Where(o => o.CreatedAt.HasValue && o.CreatedAt.Value.Date <= toDateTime.Date);
            }
        }

        return await query.CountAsync();
    }
}