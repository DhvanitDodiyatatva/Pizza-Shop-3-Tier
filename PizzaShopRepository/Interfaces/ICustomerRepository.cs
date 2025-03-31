using PizzaShopRepository.Models;

namespace PizzaShopRepository.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetCustomersAsync(string searchQuery, string sortColumn, string sortDirection, int page, int pageSize);
        Task<int> GetTotalCustomersCountAsync(string searchQuery);
    }
}