using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Models;

namespace PizzaShopRepository.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly PizzaShopContext _context; 
        public CustomerRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetCustomersAsync(string searchQuery, string sortColumn, string sortDirection, int page, int pageSize)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(c => c.Name.Contains(searchQuery) || 
                                       c.Email.Contains(searchQuery) || 
                                       c.PhoneNo.Contains(searchQuery));
            }

            query = sortColumn switch
            {
                "Name" => sortDirection == "asc" ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                "Date" => sortDirection == "asc" ? query.OrderBy(c => c.Date) : query.OrderByDescending(c => c.Date),
                "TotalOrders" => sortDirection == "asc" ? query.OrderBy(c => c.TotalOrders) : query.OrderByDescending(c => c.TotalOrders),
                _ => query.OrderBy(c => c.Name)
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCustomersCountAsync(string searchQuery)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(c => c.Name.Contains(searchQuery) || 
                                       c.Email.Contains(searchQuery) || 
                                       c.PhoneNo.Contains(searchQuery));
            }

            return await query.CountAsync();
        }
    }
}