namespace PizzaShopRepository.ViewModels
{
    // Base view model for Category (used for Fetch, Delete, Update)
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
    }

    // View model for creating a new Category
    public class CreateCategoryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // View model for updating an existing Category
    public class UpdateCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}