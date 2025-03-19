using Microsoft.EntityFrameworkCore;
using PizzaShopRepository.Data;
using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;



namespace PizzaShopRepository.Repositories
{
    public class SectionRepository : ISectionRepository
    {
        private readonly PizzaShopContext _context;

        public SectionRepository(PizzaShopContext context)
        {
            _context = context;
        }

        public async Task<List<Section>> GetAllSectionsAsync()
        {
            return await _context.Sections
                                 .Where(c => !c.IsDeleted)
                                 .ToListAsync();
        }

        public async Task<Section?> GetSectionByIdAsync(int id)
        {
            return await _context.Sections.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateSectionsAsync(Section section)
        {
            _context.Sections.Update(section);
            await _context.SaveChangesAsync();
        }

        public async Task AddSectionsAsync(Section section)
        {
            _context.Sections.Add(section);
            await _context.SaveChangesAsync();
        }

        public async Task<Section?> GetSection(string name)
        {
            return await _context.Sections.FirstOrDefaultAsync(s => s.Name == name);
        }
    }
}