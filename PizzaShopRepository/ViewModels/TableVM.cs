using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels
{
    public class TableViewModel
    {
        public int Id { get; set; }
        public int? SectionId { get; set; }

        [Required(ErrorMessage = "Table name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Table name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s-]+$", ErrorMessage = "Table name can only contain letters, numbers, spaces, and hyphens")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 50, ErrorMessage = "Capacity must be between 1 and 50")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string? Status { get; set; }


        public bool IsDeleted { get; set; }
    }
}