using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels;



public class ResetPassword
{
    [Required(ErrorMessage = "Email is Required")]
    public string Email { get; set; }

    [Required(ErrorMessage = "New Password is Required")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
    ErrorMessage = "Password must meet complexity requirements.")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm Password is Required")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}