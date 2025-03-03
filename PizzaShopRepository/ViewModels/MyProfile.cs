using System.ComponentModel.DataAnnotations;

namespace PizzaShopRepository.ViewModels;

public partial class MyProfile
{
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    public string? ProfileImage { get; set; }
    public string Role { get; set; } = null!;

    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Zipcode { get; set; }
}
