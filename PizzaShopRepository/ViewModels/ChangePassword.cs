using System.ComponentModel.DataAnnotations;
namespace PizzaShopRepository.ViewModels;

public class ChangePassword
{
    [Required(ErrorMessage = "Current Password is Required")]
    public required string CurrentPassword { get; set; }

    [Required(ErrorMessage = "New Password is Required")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
    ErrorMessage = "Password must meet complexity requirements.")]
    public required string NewPassword { get; set; }

    [Required]

    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public required string ConfirmPassword { get; set; }
}
