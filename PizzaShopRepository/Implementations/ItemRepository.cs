using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;

namespace PizzaShopRepository.Implementations;

public class ItemRepository : IItemRepository
{
    private readonly PizzaShopContext _context;

    public ItemRepository(PizzaShopContext context)
    {
        _context = context;
    }

    public async Task<List<Item>> GetItemsByCategoryAsync(int categoryId)
    {
        return await _context.Items
                             .Where(item => item.CategoryId == categoryId && !item.IsDeleted)
                             .ToListAsync();
    }

    public async Task<List<Item>> GetAllItemsAsync()
    {
        return await _context.Items
                             .Where(item => !item.IsDeleted)
                             .ToListAsync();
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        return await _context.Items.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    public async Task AddItemAsync(Item item)
    {
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Item item)
    {
        Console.WriteLine($"Updating Item: {item.Id} - {item.Name}"); // Debugging
        _context.Items.Update(item);
        await _context.SaveChangesAsync();
        Console.WriteLine("Item successfully updated!"); // Debugging
    }

    public async Task<Item?> GetItemByNameAsync(string name)
    {
        return await _context.Items.FirstOrDefaultAsync(i => i.Name == name);
    }

}
