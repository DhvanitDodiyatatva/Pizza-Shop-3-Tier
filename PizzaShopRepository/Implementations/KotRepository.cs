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
                .Where(o => o.OrderItems.Any()); // Ensure this doesn’t filter out items

            if (categoryId.HasValue)
            {
                query = query.Where(o => o.OrderItems.Any(oi => oi.Item.CategoryId == categoryId));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.OrderItems.Any(oi => oi.ItemStatus == status || oi.ItemStatus == "in_progress")); // Adjust if needed
            }

            return await query.ToListAsync();
        }

        public async Task<bool> UpdateOrderItemStatusesAsync(int orderId, List<(int OrderItemId, int AdjustedQuantity)> items, string newStatus)
        {
            if (items == null || !items.Any())
            {
                Console.WriteLine("No items provided for update.");
                return false;
            }

            try
            {
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == orderId && items.Select(i => i.OrderItemId).Contains(oi.Id))
                    .ToListAsync();

                if (!orderItems.Any())
                {
                    Console.WriteLine("No matching order items found for OrderId: {0}", orderId);
                    return false;
                }

                foreach (var item in orderItems)
                {
                    var updateItem = items.FirstOrDefault(i => i.OrderItemId == item.Id);
                    if (updateItem.AdjustedQuantity >= 0)
                    {
                        if (newStatus == "ready")
                        {
                            item.ReadyQuantity = updateItem.AdjustedQuantity;
                            item.ItemStatus = item.ReadyQuantity >= item.Quantity ? "ready" : "in_progress";
                            Console.WriteLine($"Updated Item {item.Id}: ReadyQuantity={item.ReadyQuantity}, Status={item.ItemStatus}");
                        }
                        else if (newStatus == "in_progress")
                        {
                            item.ReadyQuantity = item.ReadyQuantity - updateItem.AdjustedQuantity;
                            if (item.ReadyQuantity < 0)
                            {
                                item.ReadyQuantity = 0;
                            }
                            item.ItemStatus = item.ReadyQuantity < item.Quantity ? "in_progress" : "ready";
                            Console.WriteLine($"Updated Item {item.Id}: ReadyQuantity={item.ReadyQuantity}, Status={item.ItemStatus}");
                        }
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Database changes saved successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false; 
            }
        }
    }
}



