using Microsoft.AspNetCore.Mvc;
using PizzaShopServices.Interfaces;
using PizzaShopRepository.ViewModels;
using PizzaShopRepository.Models;

namespace PizzaShop.Controllers
{
    public class OrderAppController : Controller
    {
        private readonly ISectionService _sectionService;
        private readonly ITableService _tableService;
        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;

        public OrderAppController(ISectionService sectionService, ITableService tableService, IItemService itemService, ICategoryService categoryService)
        {
            _sectionService = sectionService;
            _tableService = tableService;
            _itemService = itemService;
            _categoryService = categoryService;
        }

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

            return View(viewModel);
        }


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
    }
}