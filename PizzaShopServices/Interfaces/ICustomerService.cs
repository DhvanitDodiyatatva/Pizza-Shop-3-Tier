using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopRepository.Services
{
    public interface ICustomerService
    {
        Task<CustomerPaginationViewModel> GetCustomersAsync(string searchQuery, string sortColumn, string sortDirection, int page, int pageSize);
    }
}