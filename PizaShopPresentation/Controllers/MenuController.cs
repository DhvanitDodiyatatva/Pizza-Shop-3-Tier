using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;
using PizzaShopServices.Attributes;
using PizzaShopService;

// [CustomAuthorize("super_admin, chef, account_manager")]
[CustomAuthorize("Menu", PermissionType.View, "super_admin", "account_manager")]
public class MenuController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IModifierGroupService _modifierGroupService;
    private readonly IModifierService _modifierService;
    private readonly IModifierGroupMappingService _modifierGroupMappingService;
    private readonly IItemModifierGroupService _itemModifierGroupService;

    public MenuController(ICategoryService categoryService, IItemService itemService, IModifierGroupService modifierGroupService, IModifierService modifierService, IModifierGroupMappingService modifierGroupMappingService, IItemModifierGroupService itemModifierGroupService)
    {
        _categoryService = categoryService;
        _itemService = itemService;
        _modifierGroupService = modifierGroupService;
        _modifierService = modifierService;
        _modifierGroupMappingService = modifierGroupMappingService;
        _itemModifierGroupService = itemModifierGroupService;
    }

    public IActionResult Index()
    {
        // Main menu page
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
        var modifierGroups = await _modifierGroupService.GetAllModifierGroupAsync();
        ViewBag.ModifierGroups = modifierGroups;
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
        var modifierGroups = await _modifierGroupService.GetAllModifierGroupAsync();
        ViewBag.ModifierGroups = modifierGroups;
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
    public async Task<IActionResult> GetModifierGroups()
    {
        List<ModifierGroup> modifierGroups = await _modifierGroupService.GetAllModifierGroupAsync();
        return PartialView("_ModifierGroupList", modifierGroups);
    }

    [HttpGet]
    public async Task<IActionResult> GetModifiersByGroupForItem(int modifierGroupId)
    {
        var modifiers = await _modifierGroupMappingService.GetModifiersByGroupIdAsync(modifierGroupId);
        return Json(modifiers); // Return modifiers as JSON
    }

    [HttpGet]
    public async Task<IActionResult> GetModifiers(int modifierGroupId, string searchString = "", int page = 1, int pageSize = 5)
    {
        try
        {
            // Get the list of Modifiers for the given ModifierGroupId
            var modifiers = await _modifierGroupMappingService.GetModifiersByGroupIdAsync(modifierGroupId);

            // Handle case where no modifiers are found
            if (modifiers == null || !modifiers.Any())
            {
                modifiers = new List<Modifier>(); // Return an empty list for consistency
            }

            // If a search string is provided, filter modifiers by name (case-insensitive)
            modifiers = modifiers.Where(m => !m.IsDeleted).ToList();
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                modifiers = modifiers.Where(m => m.Name.ToLower().Contains(searchString.ToLower())).ToList();
            }

            // Calculate paging values
            int totalItems = modifiers.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Store paging info in ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;
            ViewBag.ModifierGroupId = modifierGroupId;

            // Paginate the list
            modifiers = modifiers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Return a partial view with the list of modifiers
            return PartialView("_ModifierList", modifiers);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddNewModifier()
    {
        var modifierGroups = await _modifierGroupService.GetAllModifierGroupAsync();
        ViewBag.ModifierGroups = modifierGroups;
        return PartialView("_AddModifier", new ModifierViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNewModifier(ModifierViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            Console.WriteLine("AddNewModifier Validation Errors: " + string.Join(" | ", errors));
            return Json(new { success = false, message = "Validation failed: " + string.Join(" ", errors) });
        }

        try
        {
            await _modifierService.AddModifierAsync(model);
            return Json(new { success = true, message = "Modifier added successfully!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AddNewModifier Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error adding modifier: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditModifier(int id)
    {
        var model = await _modifierService.GetModifierForEditAsync(id);
        if (model == null)
            return NotFound("Modifier not found.");

        var modifierGroups = await _modifierGroupService.GetAllModifierGroupAsync();
        ViewBag.ModifierGroups = modifierGroups;
        return PartialView("_EditModifier", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateModifier(ModifierViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            Console.WriteLine("UpdateModifier Validation Errors: " + string.Join(" | ", errors));
            return Json(new { success = false, message = "Validation failed: " + string.Join(" ", errors) });
        }

        try
        {
            await _modifierService.UpdateModifierAsync(model);
            return Json(new { success = true, message = "Modifier updated successfully!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateModifier Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return Json(new { success = false, message = $"Error updating modifier: {ex.Message}" });
        }
    }

    //Soft Delete a Single Modifier from a Specific Group
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModifier(int id, int modifierGroupId)
    {
        try
        {
            await _modifierService.SoftDeleteModifierFromGroupAsync(id, modifierGroupId);
            return Json(new { success = true, message = "Modifier removed from group successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    //Mass Soft Delete Modifiers from a Specific Group
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModifiers([FromBody] List<int> ids, int modifierGroupId)
    {
        if (ids == null || !ids.Any())
        {
            Console.WriteLine("No IDs received in DeleteModifiers");
            return Json(new { success = false, message = "No modifiers selected for deletion." });
        }


        try
        {
            await _modifierService.SoftDeleteModifiersFromGroupAsync(ids, modifierGroupId);
            return Json(new { success = true, message = "Modifiers removed from group successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }


    [HttpGet]
    public IActionResult AddNewModifierGroup()
    {
        return PartialView("_AddModifierGroup", new ModifierGroupViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNewModifierGroup(ModifierGroupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Validation failed: " + string.Join(" ", errors) });
        }

        try
        {
            await _modifierGroupService.AddModifierGroupAsync(model);
            return Json(new { success = true, message = "Modifier Group added successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditModifierGroup(int id)
    {
        var model = await _modifierGroupService.GetModifierGroupForEditAsync(id);
        if (model == null)
            return NotFound("Modifier Group not found.");

        return PartialView("_EditModifierGroup", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateModifierGroup(ModifierGroupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Validation failed: " + string.Join(" ", errors) });
        }

        try
        {
            await _modifierGroupService.UpdateModifierGroupAsync(model);
            return Json(new { success = true, message = "Modifier Group updated successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> SelectExistingModifiers(int[] selectedModifierIds = null)
    {
        var modifiers = await _modifierService.GetAllModifiersAsync();

        // Pass the selected modifier IDs to the view
        ViewData["SelectedModifierIds"] = selectedModifierIds?.ToList() ?? new List<int>();

        return PartialView("_SelectExistingModifiers", modifiers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModifierGroup(int id)
    {
        try
        {
            await _modifierGroupService.DeleteModifierGroupAsync(id);
            return Json(new { success = true, message = "Modifier Group deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }


}