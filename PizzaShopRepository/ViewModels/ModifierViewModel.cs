using System.ComponentModel.DataAnnotations;
using PizzaShopRepository.Models;

namespace PizzaShopRepository.ViewModels
{
    public class ModifierViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Rate is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Rate must be a positive value")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        public string Unit { get; set; } = null!;

        public string? Description { get; set; }

        public List<int> ModifierGroupIds { get; set; } = new List<int>(); // For multiple modifier groups

        public List<ModifierGroup> AvailableModifierGroups { get; set; } = new List<ModifierGroup>(); // For dropdown
    }
}