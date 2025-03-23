using System.ComponentModel.DataAnnotations;
using PizzaShopRepository.Models;

namespace PizzaShopRepository.ViewModels
{
    public class ModifierGroupViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        // List of Modifier IDs to associate with this Modifier Group
        public List<int> SelectedModifierIds { get; set; } = new List<int>();

        // Add a list of Modifier objects to hold the details of selected modifiers
        public List<Modifier> SelectedModifiers { get; set; } = new List<Modifier>();
    }
}