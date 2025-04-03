using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;

namespace PizzaShopRepository.Implementations;

public class TableRepository : ITableRepository
{
    private readonly PizzaShopContext _context;

    public TableRepository(PizzaShopContext context)
    {
        _context = context;
    }

    public async Task<List<Table>> GetTablesBySectionAsync(int sectionId)
    {
        return await _context.Tables
                              .Where(table => table.SectionId == sectionId && !table.IsDeleted)
                              .ToListAsync();
    }

    public async Task<List<Table>> GetAllTablesAsync()
    {
        return await _context.Tables
                             .Where(table => !table.IsDeleted)
                             .ToListAsync();
    }

    public async Task<Table?> GetTableByIdAsync(int id)
    {
        return await _context.Tables.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task AddTableAsync(Table table)
    {
        _context.Tables.Add(table);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTableAsync(Table table)
    {
        _context.Tables.Update(table);
        await _context.SaveChangesAsync();
    }

    public async Task<Table?> GetTableByNameAsync(string name)
    {
        return await _context.Tables.FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task SoftDeleteTablesAsync(List<int> ids)
    {
        var tables = await _context.Tables
           .Where(t => ids.Contains(t.Id) && !t.IsDeleted)
           .ToListAsync();

        if (tables.Any())
        {
            foreach (var table in tables)
            {
                table.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }
    }



}
