using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels
{
    // Base view model for Category (used for Fetch, Delete, Update)
    // public class CategoryViewModel
    // {
    //     public int Id { get; set; }
    //     public string Name { get; set; } = string.Empty;
    //     public string? Description { get; set; }
    //     public bool IsDeleted { get; set; }
    // }

    public class CrudCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Description is Required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category Name is Required")]
        public string Name { get; set; } = null!;


        public bool IsDeleted { get; set; }

    }

    // View model for creating a new Category
    // public class CreateCategoryViewModel
    // {
    //     public string Name { get; set; } = string.Empty;
    //     public string? Description { get; set; }
    // }
    // public class CreateCategoryVM
    // {
    //     [Required(ErrorMessage = "Category name is required")]
    //     [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
    //     public string Name { get; set; } = string.Empty;

    //     [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
    //     public string? Description { get; set; }
    // }

    // // View model for updating an existing Category
    // // public class UpdateCategoryViewModel
    // // {
    // //     public int Id { get; set; }
    // //     public string Name { get; set; } = string.Empty;
    // //     public string? Description { get; set; }
    // // }

    // public class UpdateCategoryVM
    // {
    //     [Required(ErrorMessage = "Category ID is required")]
    //     public int Id { get; set; }

    //     [Required(ErrorMessage = "Category name is required")]
    //     [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
    //     public string Name { get; set; } = string.Empty;

    //     [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
    //     public string? Description { get; set; }
    // }
}