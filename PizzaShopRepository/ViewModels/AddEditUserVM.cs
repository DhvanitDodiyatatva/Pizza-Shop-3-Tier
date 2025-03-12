using System.ComponentModel.DataAnnotations;
namespace PizzaShopRepository.ViewModels;

public class AddEditUserVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First Name is required.")]
    [MaxLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last Name is required.")]
    [MaxLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Username is required.")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = null!;

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? Phone { get; set; }

    [MaxLength(100, ErrorMessage = "Address cannot exceed 100 characters.")]
    public string? Address { get; set; }


    public string? City { get; set; }

    public string? State { get; set; }


    public string? Country { get; set; }

    [MaxLength(6, ErrorMessage = "Zipcode cannot exceed 6 characters.")]
    public string? Zipcode { get; set; }

    public string? ProfileImage { get; set; }



    public bool Status { get; set; }
}