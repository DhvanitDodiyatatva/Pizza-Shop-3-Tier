using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels
{
    public class WaitingTokenViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string CustomerName { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Number of persons is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of persons must be at least 1.")]
        public int NumOfPersons { get; set; }

        [Required(ErrorMessage = "Section is required.")]
        public string SectionName { get; set; } = null!;

        public int SectionId { get; set; }
    }
}