using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels
{
    public class ModifierViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Rate is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be greater than 0.")]
        public decimal Price { get; set; }

        [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters.")]
        public string? Unit { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int? Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        // For associating with multiple ModifierGroups
        [Required(ErrorMessage = "At least one Modifier Group must be selected.")]
        public List<int> ModifierGroupIds { get; set; } = new List<int>();
    }
}