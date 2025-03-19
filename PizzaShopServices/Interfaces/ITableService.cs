using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface ITableService
{
    Task<List<Table>> GetTablesBySectionAsync(int sectionId);
    Task<List<Table>> GetAllTablesAsync();
    Task<(bool Success, string Message)> AddTableAsync(TableViewModel model);
    Task<TableViewModel?> GetTableForEditAsync(int id);
    Task<(bool Success, string Message)> UpdateTableAsync(TableViewModel model);
    Task<Table?> GetTableByIdAsync(int id);
    Task SoftDeleteTableAsync(int id);
    Task SoftDeleteTablesAsync(List<int> ids);
}
