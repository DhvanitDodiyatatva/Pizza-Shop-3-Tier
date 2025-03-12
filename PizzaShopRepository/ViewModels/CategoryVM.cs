using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels
{
    //ViewModel for CRUD operations of Category
    public class CrudCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Description is Required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category Name is Required")]
        public string Name { get; set; } = null!;


        public bool IsDeleted { get; set; }

    }

    
}