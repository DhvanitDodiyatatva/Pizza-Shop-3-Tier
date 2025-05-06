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

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderTables)
                    .ThenInclude(ot => ot.Table)
                    .ThenInclude(t => t.Section)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.OrderItemModifiers)
                    .ThenInclude(oim => oim.Modifier)
                .Include(o => o.OrderTaxes)
                    .ThenInclude(ot => ot.Tax)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OrderTable?> GetOrderTableByTableIdAsync(int tableId)
        {
            return await _context.OrderTables
                .Include(ot => ot.Order)
                .Where(ot => ot.TableId == tableId &&
                             (ot.Order.OrderStatus == "pending" || ot.Order.OrderStatus == "in_progress"))
                .OrderByDescending(ot => ot.Order.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Item?> GetItemWithModifiersAsync(int itemId)
        {
            return await _context.Items
                .Include(i => i.ItemModifierGroups)
                    .ThenInclude(img => img.ModifierGroup)
                        .ThenInclude(mg => mg.ModifierGroupMappings)
                            .ThenInclude(mgm => mgm.Modifier)
                .FirstOrDefaultAsync(i => i.Id == itemId && !i.IsDeleted);
        }
    }
}