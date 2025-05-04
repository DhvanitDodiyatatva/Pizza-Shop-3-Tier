using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PizzaShopServices.Interfaces;
using PizzaShopRepository.ViewModels;
using System.Threading.Tasks;
using PizzaShopRepository.Models;

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

        public OrderAppController(
        ISectionService sectionService,
        ITableService tableService,
        IItemService itemService,
        ICategoryService categoryService,
        IWaitingTokenService waitingTokenService,
        IKotService kotService,
        IOrderAppService orderAppService)
        {
            _sectionService = sectionService;
            _tableService = tableService;
            _itemService = itemService;
            _categoryService = categoryService;
            _waitingTokenService = waitingTokenService;
            _kotService = kotService;
            _orderAppService = orderAppService;
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
            var result = await _orderAppService.AssignTableAsync(selectedTableIds, sectionId, waitingTokenId, email, name, phoneNumber, numOfPersons);
            return Json(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> Menu()
        {
            return View(new MenuViewModel());
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
    }
}