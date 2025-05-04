using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;

namespace PizzaShopServices.Implementations;

public class TableService : ITableService
{
    private readonly ITableRepository _tableRepository;
    public TableService(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public async Task<List<Table>> GetTablesBySectionAsync(int sectionId)
    {
        return await _tableRepository.GetTablesBySectionAsync(sectionId);
    }

    public async Task<List<Table>> GetAllTablesAsync()
    {
        return await _tableRepository.GetAllTablesAsync();
    }

    public async Task<(bool Success, string Message)> AddTableAsync(TableViewModel model)
    {
        var existingTable = await _tableRepository.GetTableByNameAsync(model.Name);
        if (existingTable != null)
        {
            return (false, "Table with this name already exists!");
        }

        var table = new Table
        {
            SectionId = model.SectionId,
            Name = model.Name,
            Capacity = model.Capacity,
            Status = "available",
            IsDeleted = false

        };

        try
        {
            await _tableRepository.AddTableAsync(table);
            return (true, "Table added successfully!");
        }
        catch (Exception ex)
        {
            var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return (false, $"Failed to add table: {innerException}");
        }

    }

    public async Task<Table?> GetTableByIdAsync(int id)
    {
        return await _tableRepository.GetTableByIdAsync(id);
    }

    public async Task<TableViewModel?> GetTableForEditAsync(int id)
    {
        var table = await _tableRepository.GetTableByIdAsync(id);
        if (table == null)
            return null;

        return new TableViewModel
        {
            Id = table.Id,
            SectionId = table.SectionId,
            Name = table.Name,
            Capacity = table.Capacity,
            Status = table.Status
        };
    }

    public async Task<(bool Success, string Message)> UpdateTableAsync(TableViewModel model)
    {
        // Check if a table with the same name already exists
        var existingTable = await _tableRepository.GetTableByNameAsync(model.Name);
        if (existingTable != null && existingTable.Id != model.Id)
        {
            return (false, "Table with this name already exists. Please choose a different name.");
        }



        var table = await _tableRepository.GetTableByIdAsync(model.Id);
        if (table == null)
        {
            return (false, "Table not found.");
        }

        table.SectionId = model.SectionId;
        table.Name = model.Name;
        table.Capacity = model.Capacity;
        table.Status = model.Status;

        try
        {
            await _tableRepository.UpdateTableAsync(table);
            return (true, "Table updated successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in UpdateTableAsync: {ex.Message}"); // Debugging
            return (false, $"Failed to update table: {ex.Message}");
        }
    }



    public async Task<(bool Success, string Message)> SoftDeleteTableAsync(int id)
    {
        var table = await _tableRepository.GetTableByIdAsync(id);
        if (table == null)
        {
            return (false, "Table not found.");
        }

        // Check if the table status is Available
        if (table.Status != "available")
        {
            return (false, "Table cannot be deleted, it is reserved or occupied.");
        }

        table.IsDeleted = true;
        try
        {
            await _tableRepository.UpdateTableAsync(table);
            return (true, "Table deleted successfully!");
        }
        catch (Exception ex)
        {
            var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return (false, $"Failed to delete table: {innerException}");
        }
    }

    public async Task<(bool Success, string Message, List<int> DeletedIds)> SoftDeleteTablesAsync(List<int> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return (false, "No tables selected for deletion.", new List<int>());
        }

        var deletedIds = new List<int>();
        var nonAvailableTableIds = new List<int>();

        foreach (var id in ids)
        {
            var table = await _tableRepository.GetTableByIdAsync(id);
            if (table == null || table.IsDeleted)
            {
                continue; // Skip non-existent or already deleted tables
            }

            if (table.Status != "available")
            {
                nonAvailableTableIds.Add(id); // Track non-available tables
                continue;
            }

            table.IsDeleted = true;
            try
            {
                await _tableRepository.UpdateTableAsync(table);
                deletedIds.Add(id);
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return (false, $"Failed to delete table {id}: {innerException}", deletedIds);
            }
        }

        if (deletedIds.Count == 0)
        {
            if (nonAvailableTableIds.Count > 0)
            {
                return (false, "Some tables cannot be deleted, because they are reserved or occupied.", deletedIds);
            }
            return (false, "No valid tables were deleted.", deletedIds);
        }

        string message = nonAvailableTableIds.Count > 0
            ? "Some tables were deleted successfully, but others cannot be deleted because they are reserved or occupied."
            : "All selected tables were deleted successfully!";
        return (true, message, deletedIds);
    }



}
