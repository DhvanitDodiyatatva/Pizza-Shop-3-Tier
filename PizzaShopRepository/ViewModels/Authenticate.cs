using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels;




public class Authenticate
{
    [Required(ErrorMessage = "Email is Required")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is Required")]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}
