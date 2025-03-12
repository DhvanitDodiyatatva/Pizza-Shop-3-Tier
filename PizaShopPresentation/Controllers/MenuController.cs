using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using PizzaShopServices.Attributes;

[CustomAuthorize("super_admin, chef, account_manager")]
public class MenuController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;

    public MenuController(ICategoryService categoryService, IItemService itemService)
    {
        _categoryService = categoryService;
        _itemService = itemService;
    }

    public IActionResult Index()
    {
        // Main menu page, no view model passed here
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        List<Category> categories = await _categoryService.GetAllCategoriesAsync();
        return PartialView("_CategoryList", categories);
    }

    [HttpGet]
    public IActionResult AddNewCategory()
    {
        // Return a fresh view model if needed.
        return PartialView("_AddCategory", new CrudCategoryViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> AddNewCategory(CrudCategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Collect all validation errors
            var errors = ModelState.Values
                                   .SelectMany(v => v.Errors)
                                   .Select(e => e.ErrorMessage)
                                   .ToArray();
            return Json(new { success = false, message = string.Join(" ", errors) });
        }

        var result = await _categoryService.AddCategoryAsync(model);
        if (result.Success)
        {
            // Return JSON indicating success
            return Json(new { success = true, message = "Category Added Successfully" });
        }
        else
        {
            // Return JSON with the error message from your service
            return Json(new { success = false, message = result.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetItems(int categoryId)
    {
        List<Item> items = await _itemService.GetItemsByCategoryAsync(categoryId);
        return PartialView("_ItemList", items);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _categoryService.SoftDeleteCategoryAsync(id)
;
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        var model = await _categoryService.GetCategoryForEditAsync(id)
;
        if (model == null)
        {
            return NotFound("Category not found.");
        }
        return PartialView("_EditCategory", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCategory(CrudCategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Gather validation error messages and return them as JSON.
            var errors = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToArray();
            return Json(new { success = false, message = string.Join(" ", errors) });
        }

        var result = await _categoryService.UpdateCategoryAsync(model);
        if (result.Success)
        {
            // Return JSON indicating success.
            return Json(new { success = true, message = "Category updated successfully!" });
        }
        else
        {
            // Return JSON with the error message.
            return Json(new { success = false, message = result.Message });
        }
    }
}