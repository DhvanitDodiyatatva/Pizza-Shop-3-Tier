using Microsoft.AspNetCore.Mvc;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Attributes;
using PizzaShopServices.Interfaces;

namespace PizaShopPresentation.Controllers;

// [CustomAuthorize("super_admin, chef, account_manager")]
[CustomAuthorize("TableAndSection", PermissionType.View, "super_admin", "account_manager")]
public class TableAndSectionController : Controller
{
    private readonly ISectionService _sectionService;
    private readonly ITableService _tableService;

    public TableAndSectionController(ISectionService sectionService, ITableService tableService)
    {
        _sectionService = sectionService;
        _tableService = tableService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetSections()
    {
        List<Section> sections = await _sectionService.GetAllSectionsAsync();
        return PartialView("_SectionList", sections);
    }

    [HttpGet]
    public IActionResult AddNewSection()
    {
        return PartialView("_AddSection", new SectionViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> AddNewSection(SectionViewModel model)
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

        var result = await _sectionService.AddSectionAsync(model);
        if (result.Success)
        {
            // Return JSON indicating success
            return Json(new { success = true, message = "Section Added Successfully" });
        }
        else
        {
            // Return JSON with the error message from your service
            return Json(new { success = false, message = result.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTables(int sectionId, int page = 1, int pageSize = 5)
    {
        var tables = await _tableService.GetTablesBySectionAsync(sectionId);
        var totalTables = tables.Count;
        int totalPages = (int)Math.Ceiling(totalTables / (double)pageSize);

        var pagedTables = tables.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var viewModel = new PagedTableViewModel
        {
            Tables = pagedTables,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            TotalTables = totalTables,
            SectionId = sectionId
        };

        return PartialView("_TableList", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _sectionService.SoftDeleteSectionAsync(id);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTable(int id)
    {
        try
        {
            await _tableService.SoftDeleteTableAsync(id);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTables([FromBody] List<int> ids)
    {
        try
        {
            await _tableService.SoftDeleteTablesAsync(ids);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditSection(int id)
    {
        var model = await _sectionService.GetSectionForEditAsync(id);
        if (model == null)
        {
            return NotFound("Section not found.");
        }
        return PartialView("_EditSection", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSection(SectionViewModel model)
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

        var result = await _sectionService.UpdateSectionAsync(model);
        if (result.Success)
        {
            // Return JSON indicating success.
            return Json(new { success = true, message = "Section updated successfully!" });
        }
        else
        {
            // Return JSON with the error message.
            return Json(new { success = false, message = result.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddNewTable()
    {
        var sections = await _sectionService.GetAllSectionsAsync();
        ViewBag.Sections = sections;
        return PartialView("_AddTable", new TableViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> AddNewTable(TableViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return Json(new { success = false, message = string.Join(" ", errors) });
        }

        var result = await _tableService.AddTableAsync(model);
        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpGet]
    public async Task<IActionResult> EditTable(int id)
    {
        var model = await _tableService.GetTableForEditAsync(id);
        if (model == null)
            return NotFound("Table not found.");

        var sections = await _sectionService.GetAllSectionsAsync();
        ViewBag.Sections = sections;
        return PartialView("_EditTable", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTable(TableViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToArray();
            Console.WriteLine($"ModelState Errors: {string.Join(" | ", errors)}"); // Debugging
            return Json(new { success = false, message = string.Join(" ", errors) });
        }

        var result = await _tableService.UpdateTableAsync(model);

        if (!result.Success)
        {
            Console.WriteLine($"UpdateItemAsync Failed: {result.Message}"); // Debugging
        }

        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpGet]
    public async Task<IActionResult> SearchTables(string searchTerm, int page = 1, int pageSize = 5)
    {
        var tables = await _tableService.GetAllTablesAsync();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            tables = tables.Where(t => t.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        int totalTables = tables.Count;
        int totalPages = (int)Math.Ceiling(totalTables / (double)pageSize);

        var pagedTables = tables
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var viewModel = new PagedTableViewModel
        {
            Tables = pagedTables,
            CurrentPage = page,
            TotalPages = totalPages,
            PageSize = pageSize,
            TotalTables = totalTables,
            SectionId = 0 // No specific section for search
        };

        return PartialView("_TableList", viewModel);
    }

}
