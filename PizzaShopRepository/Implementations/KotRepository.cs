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

        public async Task<bool> UpdateOrderItemStatusesAsync(int orderId, List<(int OrderItemId, int AdjustedQuantity)> items, string newStatus)
        {
            if (items == null || !items.Any())
            {
                return false;
            }

            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId && items.Select(i => i.OrderItemId).Contains(oi.Id))
                .ToListAsync();

            if (!orderItems.Any())
            {
                return false;
            }

            foreach (var item in orderItems)
            {
                var updateItem = items.FirstOrDefault(i => i.OrderItemId == item.Id);
                if (updateItem.AdjustedQuantity > 0)
                {
                    if (newStatus == "ready")
                    {
                        item.ReadyQuantity = updateItem.AdjustedQuantity;
                        item.ItemStatus = newStatus;
                    }
                    else if (newStatus == "in_progress")
                    {
                        item.ReadyQuantity = item.Quantity - updateItem.AdjustedQuantity;
                        item.ItemStatus = newStatus;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}



