using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using System.Data;
using Npgsql;
using Dapper;
using System.Text.Json;
using PizzaShopRepository.ViewModels;

namespace PizzaShopRepository.Repositories
{
    public class KotRepository : IKotRepository
    {
        private readonly PizzaShopContext _context;
        private readonly IDbConnection _dbConnection;

        public KotRepository(PizzaShopContext context, IDbConnection dbConnection)
        {
            _context = context;
            _dbConnection = dbConnection;
        }

        // public async Task<List<Order>> GetOrdersByCategoryAndStatusAsync(int? categoryId, string status)
        // {
        //     var query = _context.Orders
        //         .Include(o => o.OrderItems)
        //             .ThenInclude(oi => oi.Item)
        //             .ThenInclude(i => i.Category)
        //         .Include(o => o.OrderItems)
        //             .ThenInclude(oi => oi.OrderItemModifiers)
        //             .ThenInclude(oim => oim.Modifier)
        //         .Include(o => o.OrderTables)
        //             .ThenInclude(ot => ot.Table)
        //             .ThenInclude(t => t.Section)
        //         .Where(o => o.OrderItems.Any());

        //     if (categoryId.HasValue)
        //     {
        //         query = query.Where(o => o.OrderItems.Any(oi => oi.Item.CategoryId == categoryId));
        //     }

        //     if (!string.IsNullOrEmpty(status))
        //     {
        //         query = query.Where(o => o.OrderItems.Any(oi => oi.ItemStatus == status || oi.ItemStatus == "in_progress"));
        //     }

        //     return await query.ToListAsync();
        // }

        public async Task<List<KotOrderViewModel>> GetOrdersByCategoryAndStatusAsync(int? categoryId, string status)
        {
            try
            {
                // Prepare parameters for the function
                var parameters = new
                {
                    p_category_id = categoryId.HasValue ? categoryId : (int?)null,
                    p_status = string.IsNullOrEmpty(status) ? null : status
                };

                // Call the PostgreSQL function using Dapper
                var results = await _dbConnection.QueryAsync<dynamic>(
                    "SELECT * FROM get_orders_by_category_and_status(@p_category_id, @p_status)",
                    parameters,
                    commandType: CommandType.Text
                );

                // Group the results by order_id to build the KotOrderViewModel
                var ordersDict = new Dictionary<int, KotOrderViewModel>();

                foreach (var row in results)
                {
                    int orderId = row.order_id;
                    if (!ordersDict.ContainsKey(orderId))
                    {
                        ordersDict[orderId] = new KotOrderViewModel
                        {
                            Id = orderId,
                            CreatedAt = row.order_created_at,
                            OrderInstructions = row.order_instructions
                        };
                    }

                    var order = ordersDict[orderId];

                    // Add order item
                    if (row.order_item_id != null)
                    {
                        var orderItem = new KotOrderItemViewModel
                        {
                            Id = row.order_item_id,
                            Quantity = row.order_item_quantity,
                            ReadyQuantity = row.order_item_ready_quantity,
                            SpecialInstructions = row.order_item_special_instructions,
                            ItemName = row.item_name,
                            CategoryName = row.category_name,
                            ModifierNames = row.modifier_names != null ? ((string[])row.modifier_names).ToList() : new List<string>()
                        };
                        order.OrderItems.Add(orderItem);
                    }

                    // Add order table (if not already added)
                    if (row.table_name != null && row.section_name != null)
                    {
                        var orderTable = new KotOrderTableViewModel
                        {
                            TableName = row.table_name,
                            SectionName = row.section_name
                        };
                        if (!order.OrderTables.Any(ot => ot.TableName == orderTable.TableName && ot.SectionName == orderTable.SectionName))
                        {
                            order.OrderTables.Add(orderTable);
                        }
                    }
                }

                return ordersDict.Values.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return new List<KotOrderViewModel>();
            }
        }
        //  public async Task<bool> UpdateOrderItemStatusesAsync(int orderId, List<(int OrderItemId, int AdjustedQuantity)> items, string newStatus)
        // {
        //     if (items == null || !items.Any())
        //     {
        //         Console.WriteLine("No items provided for update.");
        //         return false;
        //     }

        //     try
        //     {
        //         var orderItems = await _context.OrderItems
        //             .Where(oi => oi.OrderId == orderId && items.Select(i => i.OrderItemId).Contains(oi.Id))
        //             .ToListAsync();

        //         if (!orderItems.Any())
        //         {
        //             Console.WriteLine("No matching order items found for OrderId: {0}", orderId);
        //             return false;
        //         }

        //         bool hasChanges = false;
        //         foreach (var item in orderItems)
        //         {
        //             var updateItem = items.FirstOrDefault(i => i.OrderItemId == item.Id);
        //             if (updateItem.AdjustedQuantity >= 0)
        //             {
        //                 string originalItemStatus = item.ItemStatus;
        //                 int originalReadyQuantity = item.ReadyQuantity;

        //                 var entry = _context.Entry(item);
        //                 Console.WriteLine($"Initial Entity State for Item {item.Id}: {entry.State}");

        //                 if (newStatus == "ready")
        //                 {
        //                     int newReadyQuantity = Math.Min(item.ReadyQuantity + updateItem.AdjustedQuantity, item.Quantity);
        //                     item.ReadyQuantity = newReadyQuantity;
        //                     item.ItemStatus = item.ReadyQuantity >= item.Quantity ? "ready" : "in_progress";
        //                     if (item.ReadyQuantity > originalReadyQuantity && item.ReadyAt == null)
        //                     {
        //                         item.ReadyAt = DateTime.Now; // Set ReadyAt when item becomes ready
        //                     }
        //                     entry.Property(e => e.ReadyQuantity).IsModified = true;
        //                     entry.Property(e => e.ItemStatus).IsModified = true;
        //                     entry.Property(e => e.ReadyAt).IsModified = true;
        //                     Console.WriteLine($"Updated Item {item.Id} (Ready): Original ReadyQuantity={originalReadyQuantity}, AdjustedQuantity={updateItem.AdjustedQuantity}, New ReadyQuantity={item.ReadyQuantity}, Original Status={originalItemStatus}, New Status={item.ItemStatus}, ReadyAt={item.ReadyAt}");
        //                 }
        //                 else if (newStatus == "in_progress")
        //                 {
        //                     int reduction = Math.Min(updateItem.AdjustedQuantity, item.ReadyQuantity);
        //                     item.ReadyQuantity -= reduction;
        //                     item.ItemStatus = item.ReadyQuantity < item.Quantity ? "in_progress" : "ready";
        //                     if (item.ReadyQuantity == 0)
        //                     {
        //                         item.ReadyAt = null; // Reset ReadyAt if no items are ready
        //                     }
        //                     entry.Property(e => e.ReadyQuantity).IsModified = true;
        //                     entry.Property(e => e.ItemStatus).IsModified = true;
        //                     entry.Property(e => e.ReadyAt).IsModified = true;
        //                     Console.WriteLine($"Updated Item {item.Id} (In Progress): Original ReadyQuantity={originalReadyQuantity}, Reduced by {reduction}, New ReadyQuantity={item.ReadyQuantity}, Original Status={originalItemStatus}, New Status={item.ItemStatus}, ReadyAt={item.ReadyAt}");
        //                 }

        //                 Console.WriteLine($"Entity State after Update for Item {item.Id}: {entry.State}");
        //                 Console.WriteLine($"UPDATE Parameters: ReadyQuantity={item.ReadyQuantity}, ItemStatus={item.ItemStatus}, ReadyAt={item.ReadyAt}");
        //                 hasChanges = true;
        //             }
        //         }

        //         if (hasChanges)
        //         {
        //             var rowsAffected = await _context.SaveChangesAsync();
        //             Console.WriteLine($"Database changes saved: Rows affected = {rowsAffected}");
        //             return rowsAffected > 0;
        //         }
        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Repository Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
        //         return false;
        //     }
        // }

        public async Task<bool> UpdateOrderItemStatusesAsync(int orderId, List<(int OrderItemId, int AdjustedQuantity)> items, string newStatus)
        {
            if (items == null || !items.Any())
            {
                Console.WriteLine("No items provided for update.");
                return false;
            }

            try
            {
                // Prepare the items as a JSON array for the stored procedure
                var itemsJson = JsonSerializer.Serialize(items.Select(i => new { OrderItemId = i.OrderItemId, AdjustedQuantity = i.AdjustedQuantity }).ToList());

                // Define the parameters for the stored procedure
                var parameters = new
                {
                    p_order_id = orderId,
                    p_items = itemsJson,
                    p_new_status = newStatus,
                    p_success = false,
                    p_rows_affected = 0
                };

                // Call the stored procedure using Dapper
                var result = await _dbConnection.QueryAsync<(bool p_success, int p_rows_affected)>(
                    "CALL update_order_item_statuses(@p_order_id, @p_items::jsonb, @p_new_status, @p_success, @p_rows_affected)",
                    parameters,
                    commandType: CommandType.Text
                );

                // Extract the output parameters from the result
                var output = result.FirstOrDefault();
                bool success = output.p_success;
                int rowsAffected = output.p_rows_affected;

                if (success)
                {
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