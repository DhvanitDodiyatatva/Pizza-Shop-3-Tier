using PizzaShopRepository.Interfaces;
using PizzaShopRepository.Models;
using PizzaShopRepository.ViewModels;
using PizzaShopServices.Interfaces;

namespace PizzaShopServices.Implementations;

public class SectionService : ISectionService
{
    private readonly ISectionRepository _sectionRepository;
    public SectionService(ISectionRepository sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<List<Section>> GetAllSectionsAsync()
    {
        return await _sectionRepository.GetAllSectionsAsync();
    }
    public async Task<(bool Success, string Message)> AddSectionAsync(SectionViewModel model)
    {
        var existingSection = await _sectionRepository.GetSection(model.Name);
        if (existingSection != null)
        {
            return (false, "Section already exists.");
        }

        var section = new Section
        {
            Name = model.Name,
            Description = model.Description
        };

        try
        {
            await _sectionRepository.AddSectionsAsync(section);
            return (true, "Section added successfully.");
        }
        catch (Exception ex)
        {
            return (false, "Failed to add Category: " + ex.Message);
        }
    }



    public async Task<SectionViewModel?> GetSectionForEditAsync(int id)
    {
        var section = await _sectionRepository.GetSectionByIdAsync(id);

        if (section == null)
            return null;

        return new SectionViewModel
        {
            Id = section.Id,
            Name = section.Name,
            Description = section.Description
        };
    }

    public async Task<(bool Success, string Message)> UpdateSectionAsync(SectionViewModel model)
    {
        var section = await _sectionRepository.GetSectionByIdAsync(model.Id);
        if (section == null)
        {
            return (false, "Section not found.");
        }

        // Check if the category name is already taken by another category.
        var existingSection = await _sectionRepository.GetSection(model.Name);
        if (existingSection != null && existingSection.Id != model.Id)
        {
            return (false, "Section name already exists. Please choose another name.");
        }

        // Update properties from the model
        section.Name = model.Name;
        section.Description = model.Description;

        try
        {
            await _sectionRepository.UpdateSectionsAsync(section);
            return (true, "Section updated successfully!");
        }
        catch (Exception ex)
        {
            return (false, "Failed to update Section: " + ex.Message);
        }
    }

    public async Task SoftDeleteSectionAsync(int id)
    {
        var section = await _sectionRepository.GetSectionByIdAsync(id);
        if (section == null)
        {
            return;
        }

        section.IsDeleted = true;
        await _sectionRepository.UpdateSectionsAsync(section);
    }



}
