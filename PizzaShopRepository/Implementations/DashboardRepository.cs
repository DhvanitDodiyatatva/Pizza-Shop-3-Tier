using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopRepository.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly PizzaShopContext _context;

        public DashboardRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalSalesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus == "completed")
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrdersAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .CountAsync(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus == "completed");
        }

        public async Task<decimal> GetAverageOrderValueAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus == "completed")
                .ToListAsync();
            return orders.Any() ? orders.Average(o => o.TotalAmount) : 0;
        }

        public async Task<double> GetAverageWaitingTimeAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus == "completed" && o.UpdatedAt != null)
                .ToListAsync();
            if (!orders.Any()) return 0;
            var avgMinutes = orders.Average(o => (o.UpdatedAt!.Value - o.CreatedAt!.Value).TotalMinutes);
            return Math.Round(avgMinutes, 2);
        }

        public async Task<List<ChartDataViewModel>> GetRevenueDataAsync(DateTime startDate, DateTime endDate)
        {
            var revenueData = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus == "completed")
                .GroupBy(o => o.CreatedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key, // Keep as DateTime for now
                    Value = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Convert to ChartDataViewModel after fetching the data
            return revenueData.Select(x => new ChartDataViewModel
            {
                Date = x.Date.ToString("yyyy-MM-dd"),
                Value = x.Value
            }).ToList();
        }

        public async Task<List<ChartDataViewModel>> GetCustomerGrowthDataAsync(DateTime startDate, DateTime endDate)
        {
            var customerGrowthData = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus == "completed")
                .GroupBy(o => o.CreatedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key, // Keep as DateTime for now
                    Value = g.Select(o => o.CustomerId).Distinct().Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Convert to ChartDataViewModel after fetching the data
            return customerGrowthData.Select(x => new ChartDataViewModel
            {
                Date = x.Date.ToString("yyyy-MM-dd"),
                Value = x.Value
            }).ToList();
        }

        public async Task<List<SellingItemViewModel>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int topN = 2)
        {
            return await _context.OrderItems
                .Include(oi => oi.Item)
                .Where(oi => oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate && oi.Order.OrderStatus == "completed")
                .GroupBy(oi => oi.Item.Name)
                .Select(g => new SellingItemViewModel
                {
                    ItemName = g.Key,
                    Quantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(topN)
                .ToListAsync();
        }

        public async Task<List<SellingItemViewModel>> GetLeastSellingItemsAsync(DateTime startDate, DateTime endDate, int leastN = 2)
        {
            return await _context.OrderItems
                .Include(oi => oi.Item)
                .Where(oi => oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate && oi.Order.OrderStatus == "completed")
                .GroupBy(oi => oi.Item.Name)
                .Select(g => new SellingItemViewModel
                {
                    ItemName = g.Key,
                    Quantity = g.Sum(oi => oi.Quantity)
                })
                .OrderBy(x => x.Quantity)
                .Take(leastN)
                .ToListAsync();
        }

        public async Task<int> GetWaitingListCountAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.WaitingTokens
                .CountAsync(wt => wt.CreatedAt >= startDate && wt.CreatedAt <= endDate && !wt.IsDeleted && !wt.IsAssigned);
        }

        public async Task<int> GetNewCustomerCountAsync(DateTime startDate, DateTime endDate)
        {
            var firstOrders = await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus == "completed")
                .GroupBy(o => o.CustomerId)
                .Select(g => g.OrderBy(o => o.CreatedAt).First())
                .ToListAsync();
            return firstOrders.Count;
        }
    }
}