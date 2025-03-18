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
    private readonly IModifierGroupService _modifierGroupService;
    private readonly IModifierService _modifierService;

    public MenuController(ICategoryService categoryService, IItemService itemService, IModifierGroupService modifierGroupService, IModifierService modifierService)
    {
        _categoryService = categoryService;
        _itemService = itemService;
        _modifierGroupService = modifierGroupService;
        _modifierService = modifierService;
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
    public async Task<IActionResult> GetItems(int categoryId, int page = 1, int pageSize = 5)
    {
        var items = await _itemService.GetItemsByCategoryAsync(categoryId);
        int totalItems = items.Count;
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var pagedItems = items
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var viewModel = new PagedItemViewModel
        {
            Items = pagedItems,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            TotalItems = totalItems,
            CategoryId = categoryId
        };

        return PartialView("_ItemList", viewModel);
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

    [HttpPost]
    public async Task<IActionResult> DeleteItem(int id)
    {
        try
        {
            await _itemService.SoftDeleteItemAsync(id);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteItems([FromBody] List<int> ids)
    {
        try
        {
            await _itemService.SoftDeleteItemsAsync(ids);
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

    [HttpGet]
    public async Task<IActionResult> AddNewItem()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = categories;
        return PartialView("_AddItem", new ItemVM());
    }

    [HttpPost]
    public async Task<IActionResult> AddNewItem(ItemVM model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return Json(new { success = false, message = string.Join(" ", errors) });
        }

        var result = await _itemService.AddItemAsync(model);
        return Json(new { success = result.Success, message = result.Message });
    }


    // public async Task<IActionResult> GetItem(int id)
    // {
    //     var item = await _itemService.GetItemByIdAsync(id); // Fetch the updated item
    //     if (item == null)
    //     {
    //         return NotFound();
    //     }
    //     return PartialView("_ItemPartial", item); // Return a partial view
    // }



    [HttpGet]
    public async Task<IActionResult> EditItem(int id)
    {
        var model = await _itemService.GetItemForEditAsync(id);
        if (model == null)
            return NotFound("Item not found.");

        var categories = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Categories = categories;
        return PartialView("_EditItem", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateItem(ItemVM model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToArray();
            Console.WriteLine($"ModelState Errors: {string.Join(" | ", errors)}"); // Debugging
            return Json(new { success = false, message = string.Join(" ", errors) });
        }

        var result = await _itemService.UpdateItemAsync(model);

        if (!result.Success)
        {
            Console.WriteLine($"UpdateItemAsync Failed: {result.Message}"); // Debugging
        }

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpGet]
    public async Task<IActionResult> SearchItems(string searchTerm, int page = 1, int pageSize = 5)
    {
        var items = await _itemService.GetAllItemsAsync();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            items = items.Where(i => i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        int totalItems = items.Count;
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var pagedItems = items
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var viewModel = new PagedItemViewModel
        {
            Items = pagedItems,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            TotalItems = totalItems,
            CategoryId = 0 // No specific category for search
        };

        return PartialView("_ItemList", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetModifierGroup()
    {
        List<ModifierGroup> modifierGroups = await _modifierGroupService.GetAllModifierGrpAsync();
        return PartialView("_ModifierGroupList", modifierGroups);
    }

    [HttpGet]
    public async Task<IActionResult> GetModifiers(int modifierGroupId, int page = 1, int pageSize = 5)
    {
        var modifiers = await _modifierService.GetModifiersByModifierGrpAsync(modifierGroupId);
        int totalModifiers = modifiers.Count;
        int totalPages = (int)Math.Ceiling(totalModifiers / (double)pageSize);

        var pagedModifiers = modifiers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var viewModel = new PagedModifierViewModel
        {
            Modifiers = pagedModifiers,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            TotalModifiers = totalModifiers,
            ModifierGroupId = modifierGroupId
        };

        return PartialView("_ModifierList", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> SearchItemsForModifier(string searchTerm, int page = 1, int pageSize = 5)
    {

        var modifiers = await _modifierService.GetAllModifiersAsync();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            modifiers = modifiers.Where(i => i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        int totalModifiers = modifiers.Count;
        int totalPages = (int)Math.Ceiling(totalModifiers / (double)pageSize);

        var pagedModifiers = modifiers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var viewModel = new PagedModifierViewModel
        {
            Modifiers = pagedModifiers,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            TotalModifiers = totalModifiers,
            ModifierGroupId = 0 // No specific category for search
        };

        return PartialView("_ModifierList", viewModel);
    }

}