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
        private readonly PizzaShopRepository.Data.PizzaShopContext _context;

        public OrderAppController(ISectionService sectionService, ITableService tableService, IItemService itemService, ICategoryService categoryService, IWaitingTokenService waitingTokenService, IKotService kotService, PizzaShopRepository.Data.PizzaShopContext context)
        {
            _sectionService = sectionService;
            _tableService = tableService;
            _itemService = itemService;
            _categoryService = categoryService;
            _waitingTokenService = waitingTokenService;
            _kotService = kotService;
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

        [HttpGet]
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

            Console.WriteLine($"ShowKotDetailsModal - orderId: {orderId}, status: {status}, selectedCategory: {selectedCategory}, selectedStatus: {selectedStatus}"); // Debug log
            var viewModel = new UpdateOrderItemStatusViewModel
            {
                OrderId = order.Id,
                OrderItems = order.OrderItems
                    .Where(oi =>
                        (status == "in_progress" ? oi.Quantity > oi.ReadyQuantity : oi.ReadyQuantity > 0) && // Status filter
                        (string.IsNullOrEmpty(selectedCategory) || selectedCategory == "All" || oi.Item.Category?.Name == selectedCategory) // Category filter
                    )
                    .Select(oi => new OrderItemDetail
                    {
                        OrderItemId = oi.Id,
                        ItemName = oi.Item.Name,
                        Quantity = status == "ready" ? oi.ReadyQuantity : oi.Quantity - oi.ReadyQuantity,
                        ReadyQuantity = oi.ReadyQuantity,
                        Status = oi.ItemStatus,
                        Modifiers = oi.OrderItemModifiers
                            .Where(oim => !string.IsNullOrEmpty(oim.Modifier?.Name))
                            .Select(oim => oim.Modifier.Name)
                            .ToList(),
                        IsSelected = true,
                        AdjustedQuantity = status == "ready" ? oi.ReadyQuantity : oi.Quantity - oi.ReadyQuantity,
                        CategoryName = oi.Item.Category?.Name
                    }).ToList()
            };

            ViewBag.SelectedCategory = selectedCategory;
            ViewBag.ItemStatus = status;
            ViewBag.SelectedStatus = selectedStatus; // Pass the active status to the modal
            return PartialView("_KotDetailsModal", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderItemStatus(UpdateOrderItemStatusViewModel model, string newStatus)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_KotDetailsModal", model);
            }

            var selectedItems = model.OrderItems
                .Where(oi => oi.IsSelected && oi.AdjustedQuantity > 0)
                .Select(oi => (OrderItemId: oi.OrderItemId, AdjustedQuantity: oi.AdjustedQuantity))
                .ToList();

            if (!selectedItems.Any())
            {
                ModelState.AddModelError("", "Please select at least one item with a valid quantity.");
                return PartialView("_KotDetailsModal", model);
            }

            var result = await _kotService.UpdateOrderItemStatusesAsync(model.OrderId, selectedItems, newStatus);

            if (result)
            {
                return Json(new { success = true, message = "Item statuses updated successfully." });
            }

            return Json(new { success = false, message = "Failed to update item statuses." });
        }

    }
}