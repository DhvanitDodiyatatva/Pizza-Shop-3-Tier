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

                bool hasChanges = false;
                foreach (var item in orderItems)
                {
                    var updateItem = items.FirstOrDefault(i => i.OrderItemId == item.Id);
                    if (updateItem.AdjustedQuantity >= 0)
                    {
                        string originalItemStatus = item.ItemStatus;
                        int originalReadyQuantity = item.ReadyQuantity;

                        var entry = _context.Entry(item);
                        Console.WriteLine($"Initial Entity State for Item {item.Id}: {entry.State}");

                        if (newStatus == "ready")
                        {
                            // Increment ready_quantity by AdjustedQuantity if it exceeds current value, cap at quantity
                            int newReadyQuantity = Math.Min(item.ReadyQuantity + updateItem.AdjustedQuantity, item.Quantity);
                            item.ReadyQuantity = newReadyQuantity;
                            item.ItemStatus = item.ReadyQuantity >= item.Quantity ? "ready" : "in_progress";
                            entry.Property(e => e.ReadyQuantity).IsModified = true;
                            entry.Property(e => e.ItemStatus).IsModified = true;
                            Console.WriteLine($"Updated Item {item.Id} (Ready): Original ReadyQuantity={originalReadyQuantity}, AdjustedQuantity={updateItem.AdjustedQuantity}, New ReadyQuantity={item.ReadyQuantity}, Original Status={originalItemStatus}, New Status={item.ItemStatus}");
                        }
                        else if (newStatus == "in_progress")
                        {
                            int reduction = Math.Min(updateItem.AdjustedQuantity, item.ReadyQuantity);
                            item.ReadyQuantity -= reduction;
                            item.ItemStatus = item.ReadyQuantity < item.Quantity ? "in_progress" : "ready";
                            entry.Property(e => e.ReadyQuantity).IsModified = true;
                            entry.Property(e => e.ItemStatus).IsModified = true;
                            Console.WriteLine($"Updated Item {item.Id} (In Progress): Original ReadyQuantity={originalReadyQuantity}, Reduced by {reduction}, New ReadyQuantity={item.ReadyQuantity}, Original Status={originalItemStatus}, New Status={item.ItemStatus}");
                        }

                        Console.WriteLine($"Entity State after Update for Item {item.Id}: {entry.State}");
                        Console.WriteLine($"UPDATE Parameters: ReadyQuantity={item.ReadyQuantity}, ItemStatus={item.ItemStatus}");
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    var rowsAffected = await _context.SaveChangesAsync();
                    Console.WriteLine($"Database changes saved: Rows affected = {rowsAffected}");
                    return rowsAffected > 0;
                }
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



