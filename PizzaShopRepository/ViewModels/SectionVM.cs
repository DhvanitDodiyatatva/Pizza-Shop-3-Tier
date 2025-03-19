using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels
{
    public class SectionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Section name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Section name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s-]+$", ErrorMessage = "Section name can only contain letters, numbers, spaces, and hyphens")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
    }
}