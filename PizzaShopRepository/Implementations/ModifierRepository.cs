using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;

namespace PizzaShopRepository.Implementations;

public class ModifierRepository : IModifierRepository
{
    private readonly PizzaShopContext _context;
    public ModifierRepository(PizzaShopContext context)
    {
        _context = context;
    }
    public async Task<List<Modifier>> GetAllModifiersAsync()
    {
        return await _context.Modifiers.Where(modifier => !modifier.IsDeleted).ToListAsync();
    }

    public async Task<Modifier?> GetModifierByIdAsync(int id)
    {
        return await _context.Modifiers.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
    }

    public async Task<List<Modifier>> GetModifiersByModifierGrpAsync(int modifierGroupId)
    {
        return await _context.Modifiers.Where(modifier => modifier.ModifierGroupId == modifierGroupId && !modifier.IsDeleted).ToListAsync();

    }

}
