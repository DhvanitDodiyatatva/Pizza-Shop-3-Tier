using PizzaShopRepository.Models;

namespace PizzaShopRepository.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetCustomersAsync(string searchQuery, string sortColumn,
            string sortDirection, int page, int pageSize, string timeFilter, string fromDate, string toDate);
        Task<int> GetTotalCustomersCountAsync(string searchQuery, string timeFilter,
            string fromDate, string toDate);

        IQueryable<Customer> ApplyTimeFilter(IQueryable<Customer> query, string timeFilter,
           string fromDate, string toDate);

        Task<Customer> GetCustomerHistoryAsync(int customerId);
    }
}