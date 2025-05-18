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

        public async Task<List<Customer>> GetCustomersAsync(string searchQuery, string sortColumn,
            string sortDirection, int page, int pageSize, string timeFilter, string fromDate, string toDate)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(c => c.Name.Contains(searchQuery.Trim()) ||
                                       c.Email.Contains(searchQuery) ||
                                       c.PhoneNo.Contains(searchQuery));
            }

            // Apply time filter
            query = ApplyTimeFilter(query, timeFilter, fromDate, toDate);

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

        public async Task<int> GetTotalCustomersCountAsync(string searchQuery, string timeFilter,
            string fromDate, string toDate)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(c => c.Name.Contains(searchQuery.Trim()) ||
                                       c.Email.Contains(searchQuery) ||
                                       c.PhoneNo.Contains(searchQuery));
            }

            query = ApplyTimeFilter(query, timeFilter, fromDate, toDate);

            return await query.CountAsync();
        }

        public IQueryable<Customer> ApplyTimeFilter(IQueryable<Customer> query, string timeFilter,
            string fromDate, string toDate)
        {
            switch (timeFilter?.ToLower())
            {
                case "today":
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    query = query.Where(c => c.Date >= today && c.Date < today.AddDays(1));
                    break;

                case "this_week":
                    var startOfWeek = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek));
                    query = query.Where(c => c.Date >= startOfWeek && c.Date < startOfWeek.AddDays(7));
                    break;

                case "this_month":
                    var startOfMonth = DateOnly.FromDateTime(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
                    query = query.Where(c => c.Date >= startOfMonth && c.Date < startOfMonth.AddMonths(1));
                    break;

                case "custom":
                    if (DateTime.TryParse(fromDate, out DateTime fromDt) && DateTime.TryParse(toDate, out DateTime toDt))
                    {
                        var from = DateOnly.FromDateTime(fromDt);
                        var to = DateOnly.FromDateTime(toDt);
                        query = query.Where(c => c.Date >= from && c.Date <= to);
                    }
                    break;
            }

            return query;
        }


        public async Task<Customer> GetCustomerHistoryAsync(int customerId)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }
    }
}