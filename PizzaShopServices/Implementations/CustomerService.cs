using PizzaShopRepository.Models;
using PizzaShopRepository.Repositories;
using PizzaShopRepository.ViewModels;

namespace PizzaShopRepository.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CustomerPaginationViewModel> GetCustomersAsync(string searchQuery, string sortColumn, string sortDirection, int page, int pageSize)
        {
            var customers = await _customerRepository.GetCustomersAsync(searchQuery, sortColumn, sortDirection, page, pageSize);
            var totalCount = await _customerRepository.GetTotalCustomersCountAsync(searchQuery);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new CustomerPaginationViewModel
            {
                Customers = customers,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };
        }
    }
}