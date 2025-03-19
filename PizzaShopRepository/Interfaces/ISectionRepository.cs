using PizzaShopRepository.Models;

namespace PizzaShopRepository.Interfaces;

public interface ISectionRepository
{
     Task<List<Section>> GetAllSectionsAsync();
    Task<Section?> GetSectionByIdAsync(int id);
    Task UpdateSectionsAsync(Section section);
    Task AddSectionsAsync(Section section);
    Task<Section?> GetSection(string name);
}
