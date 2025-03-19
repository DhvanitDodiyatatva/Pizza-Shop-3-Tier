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
            Status = model.Status,
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



    public async Task SoftDeleteTableAsync(int id)
    {
        var table = await _tableRepository.GetTableByIdAsync(id);
        if (table == null)
        {
            return;
        }

        table.IsDeleted = true;
        await _tableRepository.UpdateTableAsync(table);
    }

    public async Task SoftDeleteTablesAsync(List<int> ids)
    {
        await _tableRepository.SoftDeleteTablesAsync(ids);
    }



}
