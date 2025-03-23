using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels
{
    public class TaxFeeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDefault { get; set; }
    }


    public class TaxFeeAddEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Value is required.")]
        [Range(0, 1000000, ErrorMessage = "Value must be between 0 and 1,000,000.")]
        public decimal Value { get; set; }

        public bool IsEnabled { get; set; }
        public bool IsDefault { get; set; }
    }
}
