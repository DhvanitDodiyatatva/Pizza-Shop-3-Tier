using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using PizzaShopRepository.Models;

namespace PizzaShopServices.Implementations
{
    public class OrderAppService : IOrderAppService
    {
        private readonly IOrderAppRepository _orderAppRepository;
        private readonly ISectionService _sectionService;
        private readonly ITableService _tableService;
        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;
        private readonly IWaitingTokenService _waitingTokenService;
        private readonly PizzaShopRepository.Data.PizzaShopContext _context;
        private readonly IDbConnection _dbConnection;

        public OrderAppService(
            IOrderAppRepository orderAppRepository,
            ISectionService sectionService,
            ITableService tableService,
            IItemService itemService,
            ICategoryService categoryService,
            IWaitingTokenService waitingTokenService,
            PizzaShopRepository.Data.PizzaShopContext context,
            IDbConnection dbConnection)
        {
            _orderAppRepository = orderAppRepository;
            _sectionService = sectionService;
            _tableService = tableService;
            _itemService = itemService;
            _categoryService = categoryService;
            _waitingTokenService = waitingTokenService;
            _context = context;
            _dbConnection = dbConnection;
        }

        public async Task<(bool Success, string Message)> UpdateCustomerDetailsAsync(CustomerDetailsVM model)
        {
            try
            {
                // Check if email already exists for a different customer
                var existingCustomer = await _orderAppRepository.GetCustomerByEmailAsync(model.Email);
                if (existingCustomer != null && existingCustomer.Id != model.Id)
                {
                    return (false, "This email already exists.");
                }

                var customer = await _context.Customers.FindAsync(model.Id);
                if (customer == null)
                {
                    return (false, "Customer not found.");
                }

                customer.Name = model.Name;
                customer.Email = model.Email;
                customer.PhoneNo = model.PhoneNo;
                customer.NoOfPersons = model.NoOfPersons;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                return (true, "Customer details updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<CustomerDetailsVM> GetCustomerDetailsAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return null;
            }

            return new CustomerDetailsVM
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                PhoneNo = customer.PhoneNo,
                NoOfPersons = customer.NoOfPersons
            };
        }

        public async Task<(bool Success, string Message, int OrderId)> AssignTableAsync(
            int[] selectedTableIds,
            int sectionId,
            int? waitingTokenId,
            string email,
            string name,
            string phoneNumber,
            int numOfPersons)
        {
            if (selectedTableIds == null || !selectedTableIds.Any())
            {
                return (false, "No tables selected.", 0);
            }

            if (string.IsNullOrEmpty(phoneNumber) || numOfPersons <= 0)
            {
                return (false, "Invalid customer details.", 0);
            }

            if (string.IsNullOrEmpty(name))
            {
                return (false, "Customer name is required.", 0);
            }

            try
            {
                var tables = await _orderAppRepository.GetTablesByIdsAsync(selectedTableIds);
                if (tables.Any(t => t.Status != "available"))
                {
                    return (false, "One or more selected tables are no longer available.", 0);
                }

                var customer = await _orderAppRepository.GetCustomerByEmailAsync(email);
                if (customer == null)
                {
                    customer = new Customer
                    {
                        Name = name,
                        Email = email,
                        PhoneNo = phoneNumber,
                        NoOfPersons = numOfPersons,
                        Date = DateOnly.FromDateTime(DateTime.Now)
                    };
                    await _orderAppRepository.AddCustomerAsync(customer);
                }

                var order = new Order
                {
                    CustomerId = customer.Id,
                    TotalAmount = 0,
                    InvoiceNo = $"DOM0{customer.Id}",
                    OrderType = "DineIn",
                    OrderStatus = "pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _orderAppRepository.AddOrderAsync(order);

                // Add applicable taxes from TaxesFee to OrderTax
                var applicableTaxes = await _context.TaxesFees
                    .Where(tf => tf.IsEnabled && !tf.IsDeleted)
                    .ToListAsync();

                foreach (var tax in applicableTaxes)
                {
                    var orderTax = new OrderTax
                    {
                        OrderId = order.Id,
                        TaxId = tax.Id,
                        TaxPercentage = tax.Type == "percentage" ? tax.Value : null,
                        TaxFlat = tax.Type == "fixed" ? tax.Value : null,
                        IsApplied = true // Default to applied
                    };
                    _context.OrderTaxes.Add(orderTax);
                }
                await _context.SaveChangesAsync();

                if (waitingTokenId.HasValue)
                {
                    var waitingToken = await _orderAppRepository.GetWaitingTokenByIdAsync(waitingTokenId.Value);
                    if (waitingToken != null)
                    {
                        waitingToken.IsAssigned = true;
                        await _orderAppRepository.UpdateWaitingTokenAsync(waitingToken);
                    }
                }

                foreach (var tableId in selectedTableIds)
                {
                    var table = await _orderAppRepository.GetTableByIdAsync(tableId);
                    if (table != null)
                    {
                        table.Status = "reserved";
                        await _orderAppRepository.UpdateTableAsync(table);

                        var orderTable = new OrderTable
                        {
                            OrderId = order.Id,
                            TableId = tableId
                        };
                        await _orderAppRepository.AddOrderTableAsync(orderTable);
                    }
                }

                return (true, "Tables assigned successfully.", order.Id);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", 0);
            }
        }

        public async Task<List<SectionDetailsViewModel>> GetSectionDetailsAsync()
        {
            var sections = await _sectionService.GetAllSectionsAsync();
            var viewModel = new List<SectionDetailsViewModel>();

            foreach (var section in sections)
            {
                var tables = await _tableService.GetTablesBySectionAsync(section.Id);
                var sectionViewModel = new SectionDetailsViewModel
                {
                    FloorId = section.Id,
                    FloorName = section.Name,
                    TableDetails = new List<TableDetailViewModel>()
                };

                foreach (var table in tables)
                {
                    string timeSinceCreated = null;
                    DateTime? createdAt = null;
                    decimal? totalAmount = null; // Initialize TotalAmount

                    if (table.Status == "reserved" || table.Status == "occupied")
                    {
                        var allowedStatuses = new[] { "pending", "in_progress" };

                        var orderTable = await _context.OrderTables
                            .Include(ot => ot.Order)
                            .Where(ot => ot.TableId == table.Id && allowedStatuses.Contains(ot.Order.OrderStatus))
                            .OrderByDescending(ot => ot.Order.CreatedAt)
                            .FirstOrDefaultAsync();

                        if (orderTable?.Order?.CreatedAt != null)
                        {
                            createdAt = orderTable.Order.CreatedAt;
                            var timeSpan = DateTime.Now - createdAt.Value;
                            timeSinceCreated = FormatTimeSpan(timeSpan);
                            totalAmount = orderTable.Order.TotalAmount; // Fetch TotalAmount
                        }
                    }

                    sectionViewModel.TableDetails.Add(new TableDetailViewModel
                    {
                        TableId = table.Id,
                        TableName = table.Name,
                        Capacity = table.Capacity,
                        Availability = table.Status switch
                        {
                            "available" => "Available",
                            "occupied" => "Running",
                            "reserved" => "Assigned",
                            _ => "Available"
                        },
                        TimeSinceCreated = timeSinceCreated,
                        CreatedAt = createdAt,
                        TotalAmount = totalAmount // Assign TotalAmount
                    });
                }

                viewModel.Add(sectionViewModel);
            }

            return viewModel;
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            var parts = new List<string>();

            if (timeSpan.Days > 0)
                parts.Add($"{timeSpan.Days} days");
            if (timeSpan.Hours > 0)
                parts.Add($"{timeSpan.Hours} hrs");
            if (timeSpan.Minutes > 0)
                parts.Add($"{timeSpan.Minutes} min");
            if (timeSpan.Seconds > 0)
                parts.Add($"{timeSpan.Seconds} sec");

            return parts.Any() ? string.Join(" ", parts) : "0 sec";
        }

        public async Task<WaitingTokenViewModel> PrepareWaitingTokenModalAsync(int sectionId, string sectionName)
        {
            var model = new WaitingTokenViewModel
            {
                SectionId = sectionId,
                SectionName = sectionName
            };
            return model;
        }

        public async Task<CustomerDetailsViewModel> PrepareCustomerDetailsOffcanvasAsync(string sectionIds, string sectionName, string selectedTableIds)
        {
            var tableIds = selectedTableIds.Split(',').Select(int.Parse).ToList();
            var sectionIdList = sectionIds.Split(',').Select(int.Parse).ToList();

            var allTables = new List<Table>();
            foreach (var sectionId in sectionIdList)
            {
                var tables = await _tableService.GetTablesBySectionAsync(sectionId);
                allTables.AddRange(tables);
            }

            var selectedTables = allTables
                .Where(t => tableIds.Contains(t.Id))
                .Select(t => new TableDetailsViewModel
                {
                    TableId = t.Id,
                    TableName = t.Name,
                    Capacity = t.Capacity,
                    Availability = t.Status switch
                    {
                        "available" => "Available",
                        "occupied" => "Running",
                        "reserved" => "Assigned",
                        _ => "Available"
                    }
                })
                .ToList();

            var waitingTokens = await _waitingTokenService.GetAllWaitingTokensAsync();
            var sectionTokens = waitingTokens
                .Where(t => sectionIdList.Contains((int)t.SectionId) && !t.IsDeleted && !t.IsAssigned)
                .Select(t => new WaitingTokensViewModel
                {
                    Id = t.Id,
                    CustomerName = t.CustomerName,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    NumOfPersons = t.NumOfPersons
                })
                .ToList();

            return new CustomerDetailsViewModel
            {
                SectionId = sectionIdList.FirstOrDefault(),
                SectionName = sectionName,
                SelectedTables = selectedTables,
                WaitingTokens = sectionTokens
            };
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _categoryService.GetAllCategoriesAsync();
        }

        public async Task<List<Item>> GetItemsAsync(string category, string search)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            List<Item> items;

            if (category == "Favorite Items")
            {
                items = await _itemService.GetFavoriteItemsAsync();
            }
            else if (!string.IsNullOrEmpty(category) && category != "All")
            {
                var selectedCategory = categories.FirstOrDefault(c => c.Name == category);
                if (selectedCategory != null)
                {
                    items = await _itemService.GetItemsByCategoryAsync(selectedCategory.Id);
                }
                else
                {
                    items = new List<Item>();
                }
            }
            else
            {
                items = await _itemService.GetAllItemsAsync();
            }

            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(i => i.Name.ToLower().Contains(search.ToLower())).ToList();
            }

            return items;
        }

        public async Task<(List<WaitingToken> WaitingTokens, List<SelectListItem> Sections)> GetWaitingListDataAsync()
        {
            var sections = await _sectionService.GetAllSectionsAsync();
            var sectionSelectList = sections.Select(s => new SelectListItem
            {
                Value = s.Name,
                Text = s.Name
            }).ToList();
            var waitingTokens = await _waitingTokenService.GetAllWaitingTokensAsync();
            return (waitingTokens, sectionSelectList);
        }

        //   public async Task<UpdateOrderItemStatusViewModel> PrepareKotDetailsModalAsync(int orderId, string status, string selectedCategory, string selectedStatus)
        // {
        //     var order = await _context.Orders
        //         .Include(o => o.OrderItems)
        //         .ThenInclude(oi => oi.Item)
        //         .ThenInclude(i => i.Category)
        //         .Include(o => o.OrderItems)
        //         .ThenInclude(oi => oi.OrderItemModifiers)
        //         .ThenInclude(oim => oim.Modifier)
        //         .FirstOrDefaultAsync(o => o.Id == orderId);

        //     if (order == null)
        //     {
        //         return null;
        //     }

        //     var filteredItems = order.OrderItems
        //         .Where(oi =>
        //             (status == "in_progress" ? oi.Quantity > oi.ReadyQuantity : oi.ReadyQuantity > 0) &&
        //             (string.IsNullOrEmpty(selectedCategory) || selectedCategory == "All" || oi.Item.Category == null || oi.Item.Category.Name == selectedCategory)
        //         ).ToList();

        //     var viewModel = new UpdateOrderItemStatusViewModel
        //     {
        //         OrderId = order.Id,
        //         OrderItems = filteredItems
        //             .Select(oi => new OrderItemDetail
        //             {
        //                 OrderItemId = oi.Id,
        //                 ItemName = oi.Item?.Name ?? "Unknown Item",
        //                 Quantity = status == "ready" ? oi.ReadyQuantity : oi.Quantity - oi.ReadyQuantity,
        //                 ReadyQuantity = oi.ReadyQuantity,
        //                 Status = oi.ItemStatus ?? "in_progress",
        //                 Modifiers = oi.OrderItemModifiers
        //                     .Where(oim => oim.Modifier != null && !string.IsNullOrEmpty(oim.Modifier.Name))
        //                     .Select(oim => oim.Modifier.Name)
        //                     .ToList(),
        //                 IsSelected = true,
        //                 AdjustedQuantity = status == "ready" ? oi.ReadyQuantity : oi.Quantity - oi.ReadyQuantity,
        //                 CategoryName = oi.Item?.Category?.Name ?? "Unknown Category"
        //             }).ToList()
        //     };

        //     return viewModel;
        // }

        public async Task<UpdateOrderItemStatusViewModel> PrepareKotDetailsModalAsync(int orderId, string status, string selectedCategory, string selectedStatus)
        {
            try
            {
                // Call the PostgreSQL function using Dapper
                var results = await _dbConnection.QueryAsync<dynamic>(
                    "SELECT * FROM get_order_for_kot_details(@p_order_id)",
                    new { p_order_id = orderId },
                    commandType: CommandType.Text
                );

                if (!results.Any())
                {
                    return null;
                }

                var orderItems = new List<OrderItemDetail>();
                foreach (var row in results)
                {
                    // Apply status and category filters
                    bool matchesStatus = status == "in_progress" ? (row.quantity > row.ready_quantity) : (row.ready_quantity > 0);
                    bool matchesCategory = string.IsNullOrEmpty(selectedCategory) || selectedCategory == "All" || row.category_name == selectedCategory;

                    if (matchesStatus && matchesCategory)
                    {
                        var orderItem = new OrderItemDetail
                        {
                            OrderItemId = row.order_item_id,
                            ItemName = row.item_name ?? "Unknown Item",
                            Quantity = status == "ready" ? row.ready_quantity : (row.quantity - row.ready_quantity),
                            AdjustedQuantity = status == "ready" ? row.ready_quantity : (row.quantity - row.ready_quantity),
                            Modifiers = row.modifier_names != null ? ((string[])row.modifier_names).ToList() : new List<string>(),
                            IsSelected = true
                        };
                        orderItems.Add(orderItem);
                    }
                }

                var viewModel = new UpdateOrderItemStatusViewModel
                {
                    OrderId = orderId,
                    OrderItems = orderItems
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PrepareKotDetailsModalAsync: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemModifiers)
                .ThenInclude(oim => oim.Modifier)
                .Include(o => o.OrderTaxes)
                .ThenInclude(ot => ot.Tax)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<(bool Success, string Message)> SaveOrderAsync(SaveOrderViewModel model)
        {
            try
            {
                // Fetch the existing order
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.OrderItemModifiers)
                    .Include(o => o.OrderTables)
                    .Include(o => o.OrderTaxes)
                    .ThenInclude(ot => ot.Tax)
                    .FirstOrDefaultAsync(o => o.Id == model.OrderId);

                if (order == null)
                {
                    return (false, "Order not found.");
                }

                // 1. Update Order table
                order.OrderStatus = "in_progress";
                order.TotalAmount = model.TotalAmount;
                order.PaymentMethod = model.PaymentMethod;
                order.UpdatedAt = DateTime.Now;

                // 2. Update OrderTax table
                foreach (var orderTax in order.OrderTaxes)
                {
                    if (orderTax.TaxPercentage.HasValue)
                    {
                        orderTax.IsApplied = true;
                    }
                    else if (orderTax.TaxFlat.HasValue && model.TaxSettings.ContainsKey(orderTax.Tax.Name))
                    {
                        orderTax.IsApplied = model.TaxSettings[orderTax.Tax.Name];
                    }
                }
                // 3. Update Table table
                foreach (var orderTable in order.OrderTables)
                {
                    var table = await _context.Tables.FindAsync(orderTable.TableId);
                    if (table != null)
                    {
                        table.Status = "occupied";
                        _context.Tables.Update(table);
                    }
                }

                // 4 & 5. Update OrderItem and OrderItemModifier tables
                var incomingItems = model.CartItems.ToDictionary(
                    ci => ci.ItemId + "-" + string.Join(",", ci.Modifiers.OrderBy(m => m.ModifierId).Select(m => m.ModifierId)),
                    ci => ci
                );

                // Remove items that are no longer in the cart
                var existingItems = order.OrderItems.ToList();
                foreach (var existingItem in existingItems)
                {
                    var key = existingItem.ItemId + "-" + string.Join(",", existingItem.OrderItemModifiers.OrderBy(oim => oim.ModifierId).Select(oim => oim.ModifierId));
                    if (!incomingItems.ContainsKey(key))
                    {
                        _context.OrderItemModifiers.RemoveRange(existingItem.OrderItemModifiers);
                        _context.OrderItems.Remove(existingItem);
                    }
                }

                // Add or update items
                foreach (var cartItem in model.CartItems)
                {
                    var key = cartItem.ItemId + "-" + string.Join(",", cartItem.Modifiers.OrderBy(m => m.ModifierId).Select(m => m.ModifierId));
                    var existingItem = order.OrderItems.FirstOrDefault(oi =>
                        oi.ItemId == cartItem.ItemId &&
                        string.Join(",", oi.OrderItemModifiers.OrderBy(oim => oim.ModifierId).Select(oim => oim.ModifierId)) ==
                        string.Join(",", cartItem.Modifiers.OrderBy(m => m.ModifierId).Select(m => m.ModifierId)));

                    if (existingItem == null)
                    {
                        // Add new OrderItem
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ItemId = cartItem.ItemId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = cartItem.UnitPrice,
                            TotalPrice = cartItem.TotalPrice,
                            ItemStatus = "in_progress",
                            ReadyQuantity = 0,
                            CreatedAt = DateTime.Now // Set CreatedAt
                        };
                        _context.OrderItems.Add(orderItem);
                        await _context.SaveChangesAsync();

                        // Add OrderItemModifiers
                        foreach (var modifier in cartItem.Modifiers)
                        {
                            var orderItemModifier = new OrderItemModifier
                            {
                                OrderItemId = orderItem.Id,
                                ModifierId = modifier.ModifierId,
                                Quantity = modifier.Quantity,
                                Price = modifier.Price
                            };
                            _context.OrderItemModifiers.Add(orderItemModifier);
                        }
                    }
                    else
                    {
                        // Update existing OrderItem
                        bool quantityIncreased = cartItem.Quantity > existingItem.Quantity;
                        existingItem.Quantity = cartItem.Quantity;
                        existingItem.UnitPrice = cartItem.UnitPrice;
                        existingItem.TotalPrice = cartItem.TotalPrice;
                        if (quantityIncreased)
                        {
                            existingItem.ItemStatus = "in_progress";
                        }
                        _context.OrderItems.Update(existingItem);

                        // Update OrderItemModifiers
                        var existingModifiers = existingItem.OrderItemModifiers.ToList();
                        foreach (var modifier in cartItem.Modifiers)
                        {
                            var existingModifier = existingModifiers.FirstOrDefault(oim => oim.ModifierId == modifier.ModifierId);
                            if (existingModifier == null)
                            {
                                var orderItemModifier = new OrderItemModifier
                                {
                                    OrderItemId = existingItem.Id,
                                    ModifierId = modifier.ModifierId,
                                    Quantity = modifier.Quantity,
                                    Price = modifier.Price
                                };
                                _context.OrderItemModifiers.Add(orderItemModifier);
                            }
                            else
                            {
                                existingModifier.Quantity = modifier.Quantity;
                                existingModifier.Price = modifier.Price;
                                _context.OrderItemModifiers.Update(existingModifier);
                            }
                        }

                        // Remove modifiers that are no longer present
                        foreach (var existingModifier in existingModifiers)
                        {
                            if (!cartItem.Modifiers.Any(m => m.ModifierId == existingModifier.ModifierId))
                            {
                                _context.OrderItemModifiers.Remove(existingModifier);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return (true, "Order saved successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CheckOrderItemsReadyAsync(int orderId)
        {
            try
            {
                var order = await GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return (false, "Order not found.");
                }

                bool areAllItemsReady = order.OrderItems.All(oi => oi.ItemStatus == "ready");
                return (true, areAllItemsReady ? "All items are ready." : "Some items are not ready.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CheckOrderItemsInProgressAsync(int orderId)
        {
            try
            {
                var order = await GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return (false, "Order not found.");
                }

                bool areAllItemsInProgress = order.OrderItems.All(oi => oi.ItemStatus == "in_progress" && oi.ReadyQuantity == 0);
                return (true, areAllItemsInProgress ? "All items are in progress." : "Some items are not in progress.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CompleteOrderAsync(int orderId)
        {
            try
            {
                // Fetch the order with related data
                var order = await _orderAppRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return (false, "Order not found.");
                }

                // Check if all items are ready (extra validation)
                if (!order.OrderItems.All(oi => oi.ItemStatus == "ready"))
                {
                    return (false, "Cannot complete order: Some items are not ready.");
                }

                // Fetch the customer to update TotalOrders
                var customer = await _context.Customers.FindAsync(order.CustomerId);
                if (customer == null)
                {
                    return (false, "Customer not found.");
                }

                // Increment TotalOrders
                customer.TotalOrders = (customer.TotalOrders ?? 0) + 1;
                _context.Customers.Update(customer);

                // 1. Update Order table
                order.OrderStatus = "completed";
                order.PaymentStatus = "paid";
                order.UpdatedAt = DateTime.Now;
                _context.Orders.Update(order);

                // 2. Update Table table
                foreach (var orderTable in order.OrderTables)
                {
                    if (orderTable.TableId.HasValue) // Check if TableId is not null
                    {
                        var table = await _orderAppRepository.GetTableByIdAsync(orderTable.TableId.Value); // Use .Value to get the int
                        if (table != null)
                        {
                            table.Status = "available";
                            await _orderAppRepository.UpdateTableAsync(table);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                // 3. Update OrderItem table
                foreach (var orderItem in order.OrderItems)
                {
                    orderItem.ItemStatus = "served";
                    _context.OrderItems.Update(orderItem);
                }

                await _context.SaveChangesAsync();
                return (true, "Order completed successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> SaveSpecialInstructionsAsync(int orderItemId, string specialInstructions)
        {
            try
            {
                var orderItem = await _context.OrderItems.FindAsync(orderItemId);
                if (orderItem == null)
                {
                    return (false, "Order item not found.");
                }

                orderItem.SpecialInstructions = string.IsNullOrEmpty(specialInstructions) ? null : specialInstructions;
                _context.OrderItems.Update(orderItem);
                await _context.SaveChangesAsync();
                return (true, "Special instructions saved successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> SaveOrderInstructionsAsync(int orderId, string orderInstructions)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return (false, "Order not found.");
                }

                order.OrderInstructions = string.IsNullOrEmpty(orderInstructions) ? null : orderInstructions;
                order.UpdatedAt = DateTime.Now;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return (true, "Order instructions saved successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId)
        {
            try
            {
                // Fetch the order with related data
                var order = await _orderAppRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return (false, "Order not found.");
                }

                // Check if all items are in_progress (extra validation)
                if (!order.OrderItems.All(oi => oi.ItemStatus == "in_progress"))
                {
                    return (false, "Cannot cancel order: Some items are not in progress.");
                }

                // 1. Update Order table
                order.OrderStatus = "cancelled";
                order.PaymentStatus = "failed";
                order.UpdatedAt = DateTime.Now;
                _context.Orders.Update(order);

                // 2. Update Table table
                foreach (var orderTable in order.OrderTables)
                {
                    if (orderTable.TableId.HasValue) // Check if TableId is not null
                    {
                        var table = await _orderAppRepository.GetTableByIdAsync(orderTable.TableId.Value); // Use .Value to get the int
                        if (table != null)
                        {
                            table.Status = "available";
                            await _orderAppRepository.UpdateTableAsync(table);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                // 3. Update OrderItem table
                foreach (var orderItem in order.OrderItems)
                {
                    orderItem.ItemStatus = "served";
                    _context.OrderItems.Update(orderItem);
                }

                await _context.SaveChangesAsync();
                return (true, "Order canceled successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> SaveCustomerReviewAsync(CustomerReviewViewModel model)
        {
            try
            {
                var order = await _context.Orders.FindAsync(model.OrderId);
                if (order == null)
                {
                    return (false, "Order not found.");
                }

                // Save customer review
                var review = new CustomerReview
                {
                    OrderId = model.OrderId,
                    FoodRating = model.FoodRating,
                    ServiceRating = model.ServiceRating,
                    AmbienceRating = model.AmbienceRating,
                    Comment = string.IsNullOrEmpty(model.Comment) ? null : model.Comment,
                    CreatedAt = DateTime.Now
                };
                _context.CustomerReviews.Add(review);

                // Calculate average rating and update Order
                decimal averageRating = (model.FoodRating + model.ServiceRating + model.AmbienceRating) / 3m;
                order.Rating = averageRating;
                _context.Orders.Update(order);

                await _context.SaveChangesAsync();
                return (true, "Review submitted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }
    }
}