using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;

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

    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return PartialView("_CategoryList", categories);
    }

  public async Task<IActionResult> GetItems(int? categoryId, string searchTerm, int page = 1, int pageSize = 5)
{
    var allItems = await _itemService.GetAllItemsAsync();

    if (categoryId.HasValue)
    {
        allItems = allItems.Where(i => i.CategoryId == categoryId.Value).ToList();
    }

    if (!string.IsNullOrEmpty(searchTerm))
    {
        allItems = allItems.Where(i => i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    int totalCount = allItems.Count();
    if (totalCount == 0)
    {
        return PartialView("_ItemList", new ItemPaginationViewModel
        {
            Items = new List<ItemVMViewModel>(),
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = 0
        });
    }

    var paginatedItems = allItems
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    var viewModel = new ItemPaginationViewModel
    {
        Items = paginatedItems,
        CurrentPage = page,
        PageSize = pageSize,
        TotalCount = totalCount
    };

    return PartialView("_ItemList", viewModel);
}
}