using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;

namespace PizzaShopServices.Interfaces;

public interface ISectionService
{
     Task<List<Section>> GetAllSectionsAsync();
    Task<(bool Success, string Message)> AddSectionAsync(SectionViewModel model);
    Task<SectionViewModel?> GetSectionForEditAsync(int id);
    Task<(bool Success, string Message)> UpdateSectionAsync(SectionViewModel model);
    Task SoftDeleteSectionAsync(int id);
}
