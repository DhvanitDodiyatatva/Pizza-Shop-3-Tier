using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface ITableRepository
{   
    Task<List<Table>> GetTablesBySectionAsync(int sectionId);
    Task<List<Table>> GetAllTablesAsync();
    Task<Table?> GetTableByIdAsync(int id);
    Task AddTableAsync(Table table);
    Task UpdateTableAsync(Table table);
    Task<Table?> GetTableByNameAsync(string name);
    Task SoftDeleteTablesAsync(List<int> ids);    
}
