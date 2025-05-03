using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PizzaShopRepository.Repositories
{
    public class OrderAppRepository : IOrderAppRepository
    {
        private readonly PizzaShopContext _context;

        public OrderAppRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public async Task<Customer> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<WaitingToken> GetWaitingTokenByIdAsync(int waitingTokenId)
        {
            return await _context.WaitingTokens.FindAsync(waitingTokenId);
        }

        public async Task UpdateWaitingTokenAsync(WaitingToken waitingToken)
        {
            _context.WaitingTokens.Update(waitingToken);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrderTableAsync(OrderTable orderTable)
        {
            _context.OrderTables.Add(orderTable);
            await _context.SaveChangesAsync();
        }

        public async Task<Table> GetTableByIdAsync(int tableId)
        {
            return await _context.Tables.FindAsync(tableId);
        }

        public async Task UpdateTableAsync(Table table)
        {
            _context.Tables.Update(table);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Table>> GetTablesByIdsAsync(int[] tableIds)
        {
            return await _context.Tables.Where(t => tableIds.Contains(t.Id)).ToListAsync();
        }
    }
}