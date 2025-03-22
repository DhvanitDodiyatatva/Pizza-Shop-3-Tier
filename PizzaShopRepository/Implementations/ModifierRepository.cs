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
    public async Task AddModifierAsync(Modifier modifier)
    {
        _context.Modifiers.Add(modifier);
        await _context.SaveChangesAsync();
    }


    public async Task<List<Modifier>> GetAllModifierAsync()
    {
        return await _context.Modifiers.Where(m => !m.IsDeleted).ToListAsync();
    }

    public async Task<Modifier> GetModifier(string name)
    {
        return await _context.Modifiers.FirstOrDefaultAsync(m => m.Name == name);
    }


    public async Task<Modifier?> GetModifierByIdAsync(int id)
    {
        return await _context.Modifiers.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
    }

    public async Task UpdateModifierAsync(Modifier modifier)
    {
        _context.Modifiers.Update(modifier);
        await _context.SaveChangesAsync();
    }


}
