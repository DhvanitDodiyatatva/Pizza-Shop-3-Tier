using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels;

public partial class MyProfile
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = null!;

    public string? ProfileImage { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = null!;

    [Required(ErrorMessage = "Phone No is required.")]
    [MaxLength(10, ErrorMessage = "Phone No. cannot exceed 10 characters.")]
    [MinLength(10, ErrorMessage = "Phone No. must be at least 10 characters.")]
    public string? Phone { get; set; }

    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }

    [MaxLength(6, ErrorMessage = "Zipcode cannot exceed 6 characters.")]
    [MinLength(6, ErrorMessage = "Zipcode must be at least 6 characters.")]
    public string? Zipcode { get; set; }
}
