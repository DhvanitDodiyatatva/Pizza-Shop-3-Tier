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
                             .Where(item => item.CategoryId == categoryId)
                             .ToListAsync();
    }

    public async Task<List<Item>> GetAllItemsAsync()
    {
        return await _context.Items.ToListAsync();
    }

    
}
