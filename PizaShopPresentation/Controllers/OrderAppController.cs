using Microsoft.AspNetCore.Mvc;
using PizzaShopServices.Interfaces;
using PizzaShopRepository.ViewModels;
using PizzaShopRepository.Models;
using Microsoft.AspNetCore.Mvc.Rendering; //  for SelectList
using Microsoft.EntityFrameworkCore;

namespace PizzaShop.Controllers
{
    public class OrderAppController : Controller
    {
        private readonly ISectionService _sectionService;
        private readonly ITableService _tableService;
        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;
        private readonly IWaitingTokenService _waitingTokenService;
        private readonly IKotService _kotService;

        private readonly IOrderAppService _orderAppService;
        private readonly PizzaShopRepository.Data.PizzaShopContext _context;

        public OrderAppController(ISectionService sectionService, ITableService tableService, IItemService itemService, ICategoryService categoryService, IWaitingTokenService waitingTokenService, IKotService kotService, IOrderAppService orderAppService, PizzaShopRepository.Data.PizzaShopContext context)
        {
            _sectionService = sectionService;
            _tableService = tableService;
            _itemService = itemService;
            _categoryService = categoryService;
            _waitingTokenService = waitingTokenService;
            _kotService = kotService;
            _orderAppService = orderAppService;
            _context = context;
        }

        //Table
        public async Task<IActionResult> Table()
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
                    TableDetails = tables.Select(t => new TableDetailViewModel
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
                    }).ToList()
                };
                viewModel.Add(sectionViewModel);
            }
            ViewBag.Sections = new SelectList(await _sectionService.GetAllSectionsAsync(), "Name", "Name");
            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> AddWaitingToken(WaitingTokenViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sections = new SelectList(await _sectionService.GetAllSectionsAsync(), "Name", "Name", model.SectionName);
                return PartialView("_WaitingTokenModal", model);
            }

            var result = await _waitingTokenService.AddWaitingTokenAsync(model);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return PartialView("_WaitingTokenModal", model);
            }

            return Json(new { success = true, message = result.Message });
        }

        public async Task<IActionResult> ShowWaitingTokenModal(int sectionId, string sectionName)
        {
            var model = new WaitingTokenViewModel
            {
                SectionId = sectionId,
                SectionName = sectionName
            };
            var sections = await _sectionService.GetAllSectionsAsync();
            ViewBag.Sections = new SelectList(sections ?? new List<Section>(), "Name", "Name", sectionName);
            return PartialView("_WaitingTokenModal", model);
        }


        public async Task<IActionResult> ShowCustomerDetailsOffcanvas(int sectionId, string sectionName, string selectedTableIds)
        {
            var tableIds = selectedTableIds.Split(',').Select(int.Parse).ToList();
            var tables = await _tableService.GetTablesBySectionAsync(sectionId);
            var selectedTables = tables
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
                .Where(t => t.SectionId == sectionId && !t.IsDeleted && !t.IsAssigned)
                .Select(t => new WaitingTokensViewModel
                {
                    Id = t.Id,
                    CustomerName = t.CustomerName,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    NumOfPersons = t.NumOfPersons
                })
                .ToList();

            var viewModel = new CustomerDetailsViewModel
            {
                SectionId = sectionId,
                SectionName = sectionName,
                SelectedTables = selectedTables,
                WaitingTokens = sectionTokens
            };

            return PartialView("_CustomerDetailsOffcanvas", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignTable(int[] selectedTableIds, int sectionId, int? waitingTokenId, string email, string name, string phoneNumber, int numOfPersons, string sectionName)
        {
            var result = await _orderAppService.AssignTableAsync(selectedTableIds, sectionId, waitingTokenId, email, name, phoneNumber, numOfPersons);
            return Json(new { success = result.Success, message = result.Message });
        }



        //Menu
        public async Task<IActionResult> Menu()
        {
            return View(new MenuViewModel());
        }

        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.SelectedCategory = "";
            return PartialView("_CategoryList", categories);
        }

        public async Task<IActionResult> GetItems(string category, string search)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            List<Item> items;
            if (!string.IsNullOrEmpty(category) && category != "All")
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

            ViewBag.SelectedCategory = category;
            ViewBag.SearchTerm = search;
            return PartialView("_ItemList", items);
        }

        //Waiting List
        public async Task<IActionResult> WaitingList()
        {
            var sections = await _sectionService.GetAllSectionsAsync();
            ViewBag.Sections = new SelectList(sections, "Name", "Name");
            ViewBag.SelectedSection = "All"; // Default to "All" tab
            var waitingTokens = await _waitingTokenService.GetAllWaitingTokensAsync();
            return View(waitingTokens);
        }

        // Kot
        public async Task<IActionResult> Kot()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = "All";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetKotByCategoryAndStatus(string category, string status)
        {
            var orders = await _kotService.GetOrdersByCategoryAndStatusAsync(category, status);
            ViewBag.SelectedCategory = category;
            ViewBag.ItemStatus = status;
            return PartialView("_KotCardList", orders);
        }

        public async Task<IActionResult> ShowKotDetailsModal(int orderId, string status, string selectedCategory, string selectedStatus)
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
                return NotFound();
            }

            Console.WriteLine($"ShowKotDetailsModal - orderId: {orderId}, status: {status}, selectedCategory: {selectedCategory}, selectedStatus: {selectedStatus}");
            Console.WriteLine($"Raw OrderItems count: {order.OrderItems.Count}");
            foreach (var item in order.OrderItems)
            {
                Console.WriteLine($"Raw Item: Id={item.Id}, OrderId={item.OrderId}, Quantity={item.Quantity}, ReadyQuantity={item.ReadyQuantity}, Status={item.ItemStatus}");
            }

            var filteredItems = order.OrderItems
                .Where(oi =>
                    (status == "in_progress" ? oi.Quantity > oi.ReadyQuantity : oi.ReadyQuantity > 0) &&
                    (string.IsNullOrEmpty(selectedCategory) || selectedCategory == "All" || oi.Item.Category == null || oi.Item.Category.Name == selectedCategory)
                ).ToList();

            Console.WriteLine($"Filtered OrderItems count: {filteredItems.Count}");
            foreach (var item in filteredItems)
            {
                Console.WriteLine($"Filtered Item: Id={item.Id}, Quantity={item.Quantity}, ReadyQuantity={item.ReadyQuantity}, Status={item.ItemStatus}");
            }

            var viewModel = new UpdateOrderItemStatusViewModel
            {
                OrderId = order.Id,
                OrderItems = filteredItems
                    .Select(oi => new OrderItemDetail
                    {
                        OrderItemId = oi.Id,
                        ItemName = oi.Item?.Name ?? "Unknown Item", // Ensure ItemName is set
                        Quantity = status == "ready" ? oi.ReadyQuantity : oi.Quantity - oi.ReadyQuantity,
                        ReadyQuantity = oi.ReadyQuantity,
                        Status = oi.ItemStatus ?? "in_progress", // Ensure Status is set
                        Modifiers = oi.OrderItemModifiers
                            .Where(oim => oim.Modifier != null && !string.IsNullOrEmpty(oim.Modifier.Name))
                            .Select(oim => oim.Modifier.Name)
                            .ToList(),
                        IsSelected = true,
                        AdjustedQuantity = status == "ready" ? oi.ReadyQuantity : oi.Quantity - oi.ReadyQuantity,
                        CategoryName = oi.Item?.Category?.Name ?? "Unknown Category" // Ensure CategoryName is set
                    }).ToList()
            };

            Console.WriteLine($"ViewModel OrderItems count: {viewModel.OrderItems.Count}");
            foreach (var item in viewModel.OrderItems)
            {
                Console.WriteLine($"ViewModel Item: OrderItemId={item.OrderItemId}, ItemName={item.ItemName}, Status={item.Status}, CategoryName={item.CategoryName}");
            }
            ViewBag.SelectedCategory = selectedCategory;
            ViewBag.ItemStatus = status;
            ViewBag.SelectedStatus = selectedStatus;
            return PartialView("_KotDetailsModal", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderItemStatus([Bind("OrderId,OrderItems")] UpdateOrderItemStatusViewModel model, string newStatus)
        {
            Console.WriteLine("UpdateOrderItemStatus called with OrderId: {0}, newStatus: {1}", model?.OrderId ?? 0, newStatus);
            Console.WriteLine("Received OrderItems count: {0}", model?.OrderItems?.Count ?? 0);
            foreach (var item in model?.OrderItems ?? Enumerable.Empty<OrderItemDetail>())
            {
                Console.WriteLine("Received Item: OrderItemId={0}, IsSelected={1}, AdjustedQuantity={2}", item.OrderItemId, item.IsSelected, item.AdjustedQuantity);
            }

            // Ensure model is not null
            if (model == null || model.OrderItems == null)
            {
                Console.WriteLine("Model or OrderItems is null, returning JSON with error");
                return Json(new { success = false, message = "Invalid data submitted." });
            }

            // Collect selected items
            var selectedItems = model.OrderItems
                .Where(oi => oi.IsSelected && oi.AdjustedQuantity >= 0)
                .Select(oi => (OrderItemId: oi.OrderItemId, AdjustedQuantity: oi.AdjustedQuantity))
                .ToList();

            Console.WriteLine("SelectedItems count: {0}", selectedItems.Count);
            foreach (var item in selectedItems)
            {
                Console.WriteLine("SelectedItem: OrderItemId={0}, AdjustedQuantity={1}", item.OrderItemId, item.AdjustedQuantity);
            }

            if (!selectedItems.Any())
            {
                Console.WriteLine("No selected items, returning JSON with error");
                return Json(new { success = false, message = "Please select at least one item with a valid quantity." });
            }

            try
            {
                var result = await _kotService.UpdateOrderItemStatusesAsync(model.OrderId, selectedItems, newStatus);
                if (result)
                {
                    Console.WriteLine("Update successful, returning JSON");
                    return Json(new { success = true, message = "Item statuses updated successfully." });
                }
                Console.WriteLine("Update failed, returning JSON");
                return Json(new { success = false, message = "Failed to update item statuses." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex.Message);
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

    }
}


