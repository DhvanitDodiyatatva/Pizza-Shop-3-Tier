using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PizzaShopServices.Interfaces;
using PizzaShopRepository.ViewModels;
using System.Threading.Tasks;
using PizzaShopRepository.Models;
using PizzaShopRepository.Interfaces;

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
        private readonly IOrderAppRepository _orderAppRepository;

        public OrderAppController(
            ISectionService sectionService,
            ITableService tableService,
            IItemService itemService,
            ICategoryService categoryService,
            IWaitingTokenService waitingTokenService,
            IKotService kotService,
            IOrderAppService orderAppService,
            IOrderAppRepository orderAppRepository)
        {
            _sectionService = sectionService;
            _tableService = tableService;
            _itemService = itemService;
            _categoryService = categoryService;
            _waitingTokenService = waitingTokenService;
            _kotService = kotService;
            _orderAppService = orderAppService;
            _orderAppRepository = orderAppRepository;
        }

        [HttpGet]
        public async Task<IActionResult> HasModifiers(int itemId)
        {
            var item = await _orderAppRepository.GetItemWithModifiersAsync(itemId);
            if (item == null)
            {
                return Json(new { success = false, message = "Item not found." });
            }

            bool hasModifiers = item.ItemModifierGroups.Any(img => !img.IsDeleted && img.ModifierGroup != null && !img.ModifierGroup.IsDeleted);
            return Json(new { success = true, hasModifiers = hasModifiers });
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var order = await _orderAppService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                // Return the order details, including OrderInstructions
                return Json(new
                {
                    success = true,
                    OrderInstructions = order.OrderInstructions
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ShowCustomerDetailsModal(int orderId)
        {
            var order = await _orderAppService.GetOrderByIdAsync(orderId);
            if (order == null || order.CustomerId == null)
            {
                return Json(new { success = false, message = "Order or customer not found." });
            }

            var customerDetails = await _orderAppService.GetCustomerDetailsAsync(order.CustomerId.Value);
            if (customerDetails == null)
            {
                return Json(new { success = false, message = "Customer details not found." });
            }

            return PartialView("_CustomerDetailsModal", customerDetails);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomerDetails([FromBody] CustomerDetailsVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            var (success, message) = await _orderAppService.UpdateCustomerDetailsAsync(model);
            return Json(new { success = success, message = message });
        }

        public async Task<IActionResult> Table()
        {
            var viewModel = await _orderAppService.GetSectionDetailsAsync();
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

        [HttpGet]
        public async Task<IActionResult> ShowEditWaitingTokenModal(int id)
        {
            var waitingToken = await _waitingTokenService.GetWaitingTokenByIdAsync(id);
            if (waitingToken == null)
            {
                return NotFound();
            }

            var model = new WaitingTokenViewModel
            {
                Id = waitingToken.Id,
                Email = waitingToken.Email,
                CustomerName = waitingToken.CustomerName,
                PhoneNumber = waitingToken.PhoneNumber,
                NumOfPersons = waitingToken.NumOfPersons,
                SectionId = waitingToken.SectionId.Value,
                SectionName = waitingToken.Section?.Name,
                CreatedAt = waitingToken.CreatedAt,
                IsDeleted = waitingToken.IsDeleted
            };

            var sections = await _sectionService.GetAllSectionsAsync();
            ViewBag.Sections = new SelectList(sections ?? new List<Section>(), "Name", "Name", model.SectionName);
            return PartialView("_EditWaitingTokenModal", model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWaitingToken(WaitingTokenViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var sections = await _sectionService.GetAllSectionsAsync();
                ViewBag.Sections = new SelectList(sections ?? new List<Section>(), "Name", "Name", model.SectionName);
                return PartialView("_EditWaitingTokenModal", model);
            }

            var result = await _waitingTokenService.UpdateWaitingTokenAsync(model);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                var sections = await _sectionService.GetAllSectionsAsync();
                ViewBag.Sections = new SelectList(sections ?? new List<Section>(), "Name", "Name", model.SectionName);
                return PartialView("_EditWaitingTokenModal", model);
            }

            return Json(new { success = true, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmailExists(string email, int waitingTokenId)
        {
            var exists = await _waitingTokenService.IsEmailExistsAsync(email, waitingTokenId);
            return Json(!exists); // Return true if email does not exist (valid), false if it exists (invalid)
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWaitingToken(int id)
        {
            var (success, message) = await _waitingTokenService.DeleteWaitingTokenAsync(id);
            return Json(new { success = success, message = message });
        }

        public async Task<IActionResult> ShowWaitingTokenModal(int sectionId, string sectionName)
        {
            var model = await _orderAppService.PrepareWaitingTokenModalAsync(sectionId, sectionName);
            var sections = await _sectionService.GetAllSectionsAsync();
            ViewBag.Sections = new SelectList(sections ?? new List<Section>(), "Name", "Name", sectionName);
            return PartialView("_WaitingTokenModal", model);
        }

        public async Task<IActionResult> ShowCustomerDetailsOffcanvas(string sectionIds, string sectionName, string selectedTableIds)
        {
            var viewModel = await _orderAppService.PrepareCustomerDetailsOffcanvasAsync(sectionIds, sectionName, selectedTableIds);
            return PartialView("_CustomerDetailsOffcanvas", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignTable(int[] selectedTableIds, int sectionId, int? waitingTokenId, string email, string name, string phoneNumber, int numOfPersons, string sectionName)
        {
            var (success, message, orderId) = await _orderAppService.AssignTableAsync(selectedTableIds, sectionId, waitingTokenId, email, name, phoneNumber, numOfPersons);
            if (success)
            {
                return Json(new { success = true, message = message, orderId = orderId, redirectUrl = Url.Action("Menu", "OrderApp", new { orderId = orderId }) });
            }
            return Json(new { success = false, message = message });
        }

        [HttpGet]
        public async Task<IActionResult> ShowAssignTableModal(int waitingTokenId)
        {
            var waitingToken = await _waitingTokenService.GetWaitingTokenByIdAsync(waitingTokenId);
            if (waitingToken == null)
            {
                return Json(new { success = false, message = "Waiting token not found." });
            }

            var model = new AssignTableViewModel
            {
                WaitingTokenId = waitingToken.Id,
                Email = waitingToken.Email,
                Name = waitingToken.CustomerName,
                PhoneNumber = waitingToken.PhoneNumber,
                NumOfPersons = waitingToken.NumOfPersons
            };

            var sections = await _sectionService.GetAllSectionsAsync();
            ViewBag.Sections = new SelectList(sections, "Name", "Name");
            return PartialView("_AssignTableModal", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTablesBySection(string sectionName)
        {
            var section = await _sectionService.GetAllSectionsAsync();
            var selectedSection = section.FirstOrDefault(s => s.Name == sectionName);
            if (selectedSection == null)
            {
                return Json(new List<object>());
            }

            var tables = await _tableService.GetTablesBySectionAsync(selectedSection.Id);
            var availableTables = tables
                .Where(t => t.Status == "available")
                .Select(t => new { id = t.Id, name = t.Name, capacity = t.Capacity })
                .ToList();

            return Json(availableTables);
        }


        [HttpPost]
        public async Task<IActionResult> AssignTableWL(int[] selectedTableIds, int sectionId, int? waitingTokenId, string email, string name, string phoneNumber, int numOfPersons, string sectionName)
        {
            // If sectionId is 0 (not provided), fetch it using sectionName
            if (sectionId == 0 && !string.IsNullOrEmpty(sectionName))
            {
                var sections = await _sectionService.GetAllSectionsAsync();
                var section = sections.FirstOrDefault(s => s.Name == sectionName);
                if (section != null)
                {
                    sectionId = section.Id;
                }
                else
                {
                    return Json(new { success = false, message = "Section not found." });
                }
            }

            var (success, message, orderId) = await _orderAppService.AssignTableAsync(selectedTableIds, sectionId, waitingTokenId, email, name, phoneNumber, numOfPersons);
            if (success)
            {
                return Json(new { success = true, message = message, orderId = orderId, redirectUrl = Url.Action("Menu", "OrderApp", new { orderId = orderId }) });
            }
            return Json(new { success = false, message = message });
        }

        public async Task<IActionResult> Menu(int? orderId)
        {
            ViewBag.OrderId = orderId;
            if (orderId.HasValue)
            {
                var order = await _orderAppRepository.GetOrderByIdAsync(orderId.Value);
                if (order != null)
                {
                    ViewBag.Order = order;
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCartPartial(int orderId)
        {
            var order = await _orderAppRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }
            return PartialView("_AddItemCart", order);
        }

        [HttpGet]
        public async Task<IActionResult> RedirectToMenuFromTable(int tableId)
        {
            var orderTable = await _orderAppRepository.GetOrderTableByTableIdAsync(tableId);
            if (orderTable == null || orderTable.Order == null ||
                (orderTable.Order.OrderStatus != "pending" && orderTable.Order.OrderStatus != "in_progress"))
            {
                return Json(new { success = false, redirectUrl = Url.Action("Table", "OrderApp") });
            }
            return Json(new { success = true, redirectUrl = Url.Action("Menu", "OrderApp", new { orderId = orderTable.OrderId }) });
        }

        public async Task<IActionResult> GetCategories()
        {
            var categories = await _orderAppService.GetAllCategoriesAsync();
            ViewBag.SelectedCategory = "";
            return PartialView("_CategoryList", categories);
        }

        public async Task<IActionResult> GetItems(string category, string search)
        {
            var items = await _orderAppService.GetItemsAsync(category, search);
            ViewBag.SelectedCategory = category;
            ViewBag.SearchTerm = search;
            return PartialView("_ItemList", items);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int itemId)
        {
            var result = await _itemService.ToggleFavoriteAsync(itemId);
            return Json(new { success = result.Success, message = result.Message, isFavourite = result.Success ? (await _itemService.GetItemByIdAsync(itemId))?.IsFavourite : false });
        }

        public async Task<IActionResult> WaitingList()
        {
            var (waitingTokens, sections) = await _orderAppService.GetWaitingListDataAsync();
            ViewBag.Sections = new SelectList(sections, "Value", "Text");
            ViewBag.SelectedSection = "All";
            return View(waitingTokens);
        }

        public async Task<IActionResult> Kot()
        {
            var categories = await _orderAppService.GetAllCategoriesAsync();
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
            var viewModel = await _orderAppService.PrepareKotDetailsModalAsync(orderId, status, selectedCategory, selectedStatus);
            if (viewModel == null)
            {
                return NotFound();
            }

            ViewBag.SelectedCategory = selectedCategory;
            ViewBag.ItemStatus = status;
            ViewBag.SelectedStatus = selectedStatus;
            return PartialView("_KotDetailsModal", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderItemStatus([Bind("OrderId,OrderItems")] UpdateOrderItemStatusViewModel model, string newStatus)
        {
            if (model == null || model.OrderItems == null)
            {
                return Json(new { success = false, message = "Invalid data submitted." });
            }

            var selectedItems = model.OrderItems
                .Where(oi => oi.IsSelected && oi.AdjustedQuantity >= 0)
                .Select(oi => (OrderItemId: oi.OrderItemId, AdjustedQuantity: oi.AdjustedQuantity))
                .ToList();

            if (!selectedItems.Any())
            {
                return Json(new { success = false, message = "Please select at least one item with a valid quantity." });
            }

            try
            {
                var result = await _kotService.UpdateOrderItemStatusesAsync(model.OrderId, selectedItems, newStatus);
                return Json(new { success = result, message = result ? "Item statuses updated successfully." : "Failed to update item statuses." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ShowItemModifiersModal(int itemId)
        {
            var item = await _orderAppRepository.GetItemWithModifiersAsync(itemId);
            if (item == null)
            {
                return NotFound();
            }
            return PartialView("_ItemModifiers", item);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _orderAppService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            var orderItems = order.OrderItems.Select(oi => new
            {
                id = oi.Id,
                itemId = oi.ItemId,
                itemName = oi.Item.Name,
                quantity = oi.Quantity,
                unitPrice = oi.UnitPrice,
                totalPrice = oi.TotalPrice,
                readyQuantity = oi.ReadyQuantity,
                specialInstructions = oi.SpecialInstructions,
                modifiers = oi.OrderItemModifiers.Select(oim => new
                {
                    modifierId = oim.ModifierId,
                    modifierName = oim.Modifier.Name,
                    price = oim.Price
                }).ToList()
            }).ToList();

            var orderTaxes = order.OrderTaxes.Select(ot => new
            {
                taxName = ot.Tax.Name,
                taxFlat = ot.TaxFlat,
                taxPercentage = ot.TaxPercentage,
                isApplied = ot.IsApplied
            }).ToList();

            return Json(new
            {
                success = true,
                orderItems = orderItems,
                orderTaxes = orderTaxes
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveSpecialInstructions(int orderItemId, string specialInstructions)
        {
            var (success, message) = await _orderAppService.SaveSpecialInstructionsAsync(orderItemId, specialInstructions);
            return Json(new { success = success, message = message });
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrderInstructions(int orderId, string orderInstructions)
        {
            var (success, message) = await _orderAppService.SaveOrderInstructionsAsync(orderId, orderInstructions);
            return Json(new { success = success, message = message });
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrder([FromBody] SaveOrderViewModel model)
        {
            if (!ModelState.IsValid || model == null)
            {
                return Json(new { success = false, message = "Invalid order data." });
            }

            try
            {
                var result = await _orderAppService.SaveOrderAsync(model);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckOrderItemsReady(int orderId)
        {
            var (success, message) = await _orderAppService.CheckOrderItemsReadyAsync(orderId);
            if (!success)
            {
                return Json(new { success = false, message = message });
            }

            bool areAllItemsReady = message.Contains("All items are ready.");
            return Json(new { success = true, areAllItemsReady = areAllItemsReady });
        }

        [HttpGet]
        public async Task<IActionResult> CheckOrderItemsInProgress(int orderId)
        {
            var (success, message) = await _orderAppService.CheckOrderItemsInProgressAsync(orderId);
            return Json(new { success = success, message = message, areAllItemsInProgress = success && message.Contains("All items are in progress.") });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var (success, message) = await _orderAppService.CompleteOrderAsync(orderId);
            return Json(new { success = success, message = message, showReviewModal = success });
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var (success, message) = await _orderAppService.CancelOrderAsync(orderId);
            return Json(new { success = success, message = message });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitCustomerReview([FromBody] CustomerReviewViewModel model)
        {
            if (!ModelState.IsValid || model == null)
            {
                return Json(new { success = false, message = "Invalid review data." });
            }

            try
            {
                var (success, message) = await _orderAppService.SaveCustomerReviewAsync(model);
                return Json(new { success = success, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}