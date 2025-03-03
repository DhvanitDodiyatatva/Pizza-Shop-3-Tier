using System.ComponentModel.DataAnnotations;
namespace PizzaShop.ViewModels;

public class ChangePassword
{
    [Required(ErrorMessage = "Current Password is Required")]
    public required string CurrentPassword { get; set; }

    [Required(ErrorMessage = "New Password is Required")]
   
    public required string NewPassword { get; set; }

    [Required]
   
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public required string ConfirmPassword { get; set; }
}
