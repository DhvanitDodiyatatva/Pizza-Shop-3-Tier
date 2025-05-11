using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaShopRepository.Repositories
{
    public interface IDashboardRepository

    {
        Task<decimal> GetTotalSalesAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalOrdersAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetAverageOrderValueAsync(DateTime startDate, DateTime endDate);
        Task<double> GetAverageWaitingTimeAsync(DateTime startDate, DateTime endDate);
        Task<List<ChartDataViewModel>> GetRevenueDataAsync(DateTime startDate, DateTime endDate);
        Task<List<ChartDataViewModel>> GetCustomerGrowthDataAsync(DateTime startDate, DateTime endDate);
        Task<List<SellingItemViewModel>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int topN = 2);
        Task<List<SellingItemViewModel>> GetLeastSellingItemsAsync(DateTime startDate, DateTime endDate, int leastN = 2);
        Task<int> GetWaitingListCountAsync(DateTime startDate, DateTime endDate);
        Task<int> GetNewCustomerCountAsync(DateTime startDate, DateTime endDate);
    }
}