using Microsoft.AspNetCore.Mvc;
using PizzaShop.Services.Interfaces;
// using PizzaShop.ViewModels;
using PizzaShopRepository.ViewModels;
using System.Linq;

namespace PizzaShop.Controllers
{
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public IActionResult Index()
        {
            var model = new MenuViewModel
            {
                Categories = _menuService.GetAllCategories().ToList(),
                Items = _menuService.GetAllItems().ToList()
            };
            return View(model);
        }

        // AJAX endpoint to get items for a specific category
        [HttpGet]
        public IActionResult GetItemsByCategory(int? categoryId)
        {
            var items = _menuService.GetAllItems()
                .Where(i => !i.IsDeleted && (categoryId == null || i.CategoryId == categoryId))
                .ToList();
            return PartialView("_ItemList", items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(CreateCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                _menuService.CreateCategory(model);
                return RedirectToAction("Index");
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCategory(UpdateCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                _menuService.UpdateCategory(model);
                return RedirectToAction("Index");
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateItem(CreateItemVMViewModel model)
        {
            if (ModelState.IsValid)
            {
                _menuService.CreateItem(model);
                return RedirectToAction("Index");
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateItem(UpdateItemVMViewModel model)
        {
            if (ModelState.IsValid)
            {
                _menuService.UpdateItem(model);
                return RedirectToAction("Index");
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteItem(int id)
        {
            var item = _menuService.GetItemById(id);
            if (item != null)
            {
                _menuService.DeleteItem(id);
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult DeleteItems([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest("No items selected for deletion.");
            }
            _menuService.DeleteItems(ids);
            return Ok();
        }
    }
}