using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;



namespace PizzaShopRepository.Repositories
{
    public class KotRepository : IKotRepository
    {
        private readonly PizzaShopContext _context;

        public KotRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetOrdersByCategoryAndStatusAsync(int? categoryId, string status)
        {
            var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .ThenInclude(i => i.Category)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemModifiers)
                .ThenInclude(oim => oim.Modifier)
            .Include(o => o.OrderTables)
                .ThenInclude(ot => ot.Table)
                .ThenInclude(t => t.Section)
            .Where(o => o.OrderItems.Any(oi => !oi.Item.IsDeleted));

            if (categoryId.HasValue)
            {
                query = query.Where(o => o.OrderItems.Any(oi => oi.Item.CategoryId == categoryId));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.OrderItems.Any(oi => oi.ItemStatus == status));
            }

            return await query.ToListAsync();
        }
    }
}


