namespace PizzaShopRepository.ViewModels
{
    // Base view model for ItemVM (used for Fetch, Delete, Update)
    public class ItemVMViewModel
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public int? Quantity { get; set; }
        public string? Unit { get; set; }
        public bool IsAvailable { get; set; }
        public string? ShortCode { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsDeleted { get; set; }
        public string? CategoryName { get; set; } // Optional for display purposes
    }

    // View model for creating a new ItemVM
    public class CreateItemVMViewModel
    {
        public int? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public int? Quantity { get; set; }
        public string? Unit { get; set; }
        public bool? IsAvailable { get; set; }
        public string? ShortCode { get; set; }
        public string? ImageUrl { get; set; }
    }

    // View model for updating an existing ItemVM
    public class UpdateItemVMViewModel
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public int? Quantity { get; set; }
        public string? Unit { get; set; }
        public bool? IsAvailable { get; set; }
        public string? ShortCode { get; set; }
        public string? ImageUrl { get; set; }
    }
}