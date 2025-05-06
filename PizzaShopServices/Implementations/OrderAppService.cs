using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public OrderAppService(
            IOrderAppRepository orderAppRepository,
            ISectionService sectionService,
            ITableService tableService,
            IItemService itemService,
            ICategoryService categoryService,
            IWaitingTokenService waitingTokenService,
            PizzaShopRepository.Data.PizzaShopContext context)
        {
            _orderAppRepository = orderAppRepository;
            _sectionService = sectionService;
            _tableService = tableService;
            _itemService = itemService;
            _categoryService = categoryService;
            _waitingTokenService = waitingTokenService;
            _context = context;
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

                    if (table.Status == "reserved" || table.Status == "occupied")
                    {
                        var orderTable = await _context.OrderTables
                            .Include(ot => ot.Order)
                            .Where(ot => ot.TableId == table.Id && ot.Order.OrderStatus == "pending")
                            .OrderByDescending(ot => ot.Order.CreatedAt)
                            .FirstOrDefaultAsync();

                        if (orderTable?.Order?.CreatedAt != null)
                        {
                            createdAt = orderTable.Order.CreatedAt;
                            var timeSpan = DateTime.Now - createdAt.Value;
                            timeSinceCreated = FormatTimeSpan(timeSpan);
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
                        CreatedAt = createdAt
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

        public async Task<UpdateOrderItemStatusViewModel> PrepareKotDetailsModalAsync(int orderId, string status, string selectedCategory, string selectedStatus)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .ThenInclude(i => i.Category)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemModifiers)
                .ThenInclude(oim => oim.Modifier)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            var filteredItems = order.OrderItems
                .Where(oi =>
                    (status == "in_progress" ? oi.Quantity > oi.ReadyQuantity : oi.ReadyQuantity > 0) &&
                    (string.IsNullOrEmpty(selectedCategory) || selectedCategory == "All" || oi.Item.Category == null || oi.Item.Category.Name == selectedCategory)
                ).ToList();

            var viewModel = new UpdateOrderItemStatusViewModel
            {
                OrderId = order.Id,
                OrderItems = filteredItems
                    .Select(oi => new OrderItemDetail
                    {
                        OrderItemId = oi.Id,
                        ItemName = oi.Item?.Name ?? "Unknown Item",
                        Quantity = status == "ready" ? oi.ReadyQuantity : oi.Quantity - oi.ReadyQuantity,
                        ReadyQuantity = oi.ReadyQuantity,
                        Status = oi.ItemStatus ?? "in_progress",
                        Modifiers = oi.OrderItemModifiers
                            .Where(oim => oim.Modifier != null && !string.IsNullOrEmpty(oim.Modifier.Name))
                            .Select(oim => oim.Modifier.Name)
                            .ToList(),
                        IsSelected = true,
                        AdjustedQuantity = status == "ready" ? oi.ReadyQuantity : oi.Quantity - oi.ReadyQuantity,
                        CategoryName = oi.Item?.Category?.Name ?? "Unknown Category"
                    }).ToList()
            };

            return viewModel;
        }
    }
}