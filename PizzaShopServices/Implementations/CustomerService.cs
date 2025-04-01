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

        public async Task<CustomerPaginationViewModel> GetCustomersAsync(string searchQuery, string sortColumn,
            string sortDirection, int page, int pageSize, string timeFilter, string fromDate, string toDate)
        {
            var customers = await _customerRepository.GetCustomersAsync(searchQuery, sortColumn,
                sortDirection, page, pageSize, timeFilter, fromDate, toDate);

            var totalCount = await _customerRepository.GetTotalCustomersCountAsync(searchQuery, timeFilter,
                fromDate, toDate);

            return new CustomerPaginationViewModel
            {
                Customers = customers,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };
        }
    }

}