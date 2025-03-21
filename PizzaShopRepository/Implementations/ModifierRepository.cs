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
        return await _context.Modifiers.ToListAsync();
    }

    public async Task<Modifier> GetModifier(string name)
    {
        return await _context.Modifiers.FirstOrDefaultAsync(m => m.Name == name);
    }


    public async Task<Modifier?> GetModifierByIdAsync(int id)
    {
        return await _context.Modifiers.FirstOrDefaultAsync(m => m.Id == id);
    }
    // public async Task<List<Modifier>> GetModifiersByModifierGrpAsync(int modifierGroupId)
    // {
    //     // return await _context.Modifiers.Where(modifier => modifier.ModifierGroupId == modifierGroupId && !modifier.IsDeleted).ToListAsync();

    // }

}
