using Microsoft.AspNetCore.Mvc;
using PizzaShopServices.Interfaces;
using PizzaShopRepository.ViewModels;
using PizzaShopRepository.Models;
using Microsoft.AspNetCore.Mvc.Rendering; // Added for SelectList

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

        public OrderAppController(ISectionService sectionService, ITableService tableService, IItemService itemService, ICategoryService categoryService, IWaitingTokenService waitingTokenService, IKotService kotService)
        {
            _sectionService = sectionService;
            _tableService = tableService;
            _itemService = itemService;
            _categoryService = categoryService;
            _waitingTokenService = waitingTokenService;
            _kotService = kotService;
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

    }
}