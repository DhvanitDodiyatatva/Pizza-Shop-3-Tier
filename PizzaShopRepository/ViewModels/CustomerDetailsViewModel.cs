using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels
{
    public class CustomerDetailsViewModel
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = null!;
        public List<TableDetailsViewModel> SelectedTables { get; set; } = new List<TableDetailsViewModel>();
        public List<WaitingTokensViewModel> WaitingTokens { get; set; } = new List<WaitingTokensViewModel>();

        [Required(ErrorMessage = "Name is required.")]
        public string CustomerName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [MaxLength(10, ErrorMessage = "Phone No. cannot exceed 10 characters.")]
        [MinLength(10, ErrorMessage = "Phone No. must be at least 10 characters.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Number of persons is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of persons must be at least 1.")]
        public int NumOfPersons { get; set; }
    }

    public class WaitingTokensViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string CustomerName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [MaxLength(10, ErrorMessage = "Phone No. cannot exceed 10 characters.")]
        [MinLength(10, ErrorMessage = "Phone No. must be at least 10 characters.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Number of persons is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of persons must be at least 1.")]
        public int NumOfPersons { get; set; }
    }

    public class TableDetailsViewModel
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = null!;
        public int Capacity { get; set; }
        public string Availability { get; set; } = null!;
    }
}