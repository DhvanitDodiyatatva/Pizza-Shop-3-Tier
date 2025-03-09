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

    public async Task<IEnumerable<Item>> GetAllItemsAsync()
    {
        return await _context.Items.Where(i => !i.IsDeleted).ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetItemsByCategoryIdAsync(int categoryId)
    {
        return await _context.Items.Where(i => i.CategoryId == categoryId && !i.IsDeleted).ToListAsync();
    }

    public async Task<Item> GetItemByIdAsync(int id)
    {
        return await _context.Items.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }
}
