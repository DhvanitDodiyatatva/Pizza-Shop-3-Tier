using System.ComponentModel.DataAnnotations;
namespace PizzaShopRepository.ViewModels
{
    // Base view model for ItemVM (used for Fetch, Delete, Update)
    public class ItemVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(1, 9999, ErrorMessage = "Price must be between 1 and 9999")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
        public string ItemType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 1")]
        public int? Quantity { get; set; }
        public string? Unit { get; set; }

        [Required(ErrorMessage = "Availability status is required")]
        public bool IsAvailable { get; set; }

        [StringLength(10, ErrorMessage = "Short code cannot exceed 10 characters")]
        public string? ShortCode { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsDeleted { get; set; }
        public string? CategoryName { get; set; }

        [Range(0, 100, ErrorMessage = "Tax percentage must be between 0 and 100")]
        public decimal? TaxPercentage { get; set; }

        public bool DefaultTax { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<int> SelectedModifierGroupIds { get; set; } = new List<int>();
        public List<ModifierGroupConfig> ModifierGroupConfigs { get; set; } = new List<ModifierGroupConfig>();
    }

    public class ModifierGroupConfig
    {
        public int ModifierGroupId { get; set; }
        public int? MinLoad { get; set; }
        public int? MaxLoad { get; set; }
    }


}