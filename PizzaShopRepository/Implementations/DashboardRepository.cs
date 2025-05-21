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
                .Include(o => o.OrderItems)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus == "completed")
                .ToListAsync();

            var completedOrders = orders
                .Where(o => o.OrderItems.Any(oi => oi.ReadyQuantity > 0 && oi.ReadyAt.HasValue))
                .ToList();

            var waitingTimes = completedOrders.Select(o =>
            {
                var createdItems = o.OrderItems.Where(oi => oi.CreatedAt.HasValue);
                if (createdItems.Any())
                {
                    var earliestCreatedAt = createdItems.Min(oi => oi.CreatedAt.Value);
                    var readyItems = o.OrderItems.Where(oi => oi.ReadyQuantity > 0 && oi.ReadyAt.HasValue);
                    if (readyItems.Any())
                    {
                        var earliestReadyAt = readyItems.Min(oi => oi.ReadyAt.Value);
                        var waitingTime = (earliestReadyAt - earliestCreatedAt).TotalMinutes;
                        return waitingTime > 0 ? waitingTime : 0;
                    }
                }
                return 0.0;
            }).Where(t => t > 0).ToList();

            return waitingTimes.Any() ? Math.Round(waitingTimes.Average(), 2) : 0;
        }

        public async Task<List<ChartDataViewModel>> GetRevenueDataAsync(DateTime startDate, DateTime endDate, string groupBy = "day")
        {
            IQueryable<Order> query = _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus == "completed");

            if (groupBy == "month")
            {
                var revenueData = await query
                    .GroupBy(o => new { o.CreatedAt!.Value.Year, o.CreatedAt.Value.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Value = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();

                var allMonths = new List<ChartDataViewModel>();
                var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
                var endMonth = new DateTime(endDate.Year, endDate.Month, 1);
                while (currentDate <= endMonth)
                {
                    var year = currentDate.Year;
                    var month = currentDate.Month;
                    var revenueEntry = revenueData.FirstOrDefault(r => r.Year == year && r.Month == month);
                    allMonths.Add(new ChartDataViewModel
                    {
                        Date = currentDate.ToString("yyyy-MM"),
                        Value = revenueEntry?.Value ?? 0
                    });
                    currentDate = currentDate.AddMonths(1);
                }
                return allMonths;
            }
            else
            {
                var revenueData = await query
                    .GroupBy(o => o.CreatedAt!.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Value = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var allDates = new List<ChartDataViewModel>();
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var dateStr = date.ToString("yyyy-MM-dd");
                    var revenueEntry = revenueData.FirstOrDefault(r => r.Date.Date == date.Date);
                    allDates.Add(new ChartDataViewModel
                    {
                        Date = dateStr,
                        Value = revenueEntry?.Value ?? 0
                    });
                }
                return allDates;
            }
        }

        public async Task<List<ChartDataViewModel>> GetCustomerGrowthDataAsync(DateTime startDate, DateTime endDate, string groupBy = "day")
        {
            IQueryable<Order> query = _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.OrderStatus == "completed");

            if (groupBy == "month")
            {
                var customerData = await query
                    .GroupBy(o => new { o.CreatedAt!.Value.Year, o.CreatedAt.Value.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Value = g.Select(o => o.CustomerId).Distinct().Count()
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();

                var allMonths = new List<ChartDataViewModel>();
                var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
                var endMonth = new DateTime(endDate.Year, endDate.Month, 1);
                while (currentDate <= endMonth)
                {
                    var year = currentDate.Year;
                    var month = currentDate.Month;
                    var customerEntry = customerData.FirstOrDefault(c => c.Year == year && c.Month == month);
                    allMonths.Add(new ChartDataViewModel
                    {
                        Date = currentDate.ToString("yyyy-MM"),
                        Value = customerEntry?.Value ?? 0
                    });
                    currentDate = currentDate.AddMonths(1);
                }
                return allMonths;
            }
            else
            {
                var customerGrowthData = await query
                    .GroupBy(o => o.CreatedAt!.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Value = g.Select(o => o.CustomerId).Distinct().Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var allDates = new List<ChartDataViewModel>();
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var dateStr = date.ToString("yyyy-MM-dd");
                    var growthEntry = customerGrowthData.FirstOrDefault(g => g.Date.Date == date.Date);
                    allDates.Add(new ChartDataViewModel
                    {
                        Date = dateStr,
                        Value = growthEntry?.Value ?? 0
                    });
                }
                return allDates;
            }
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