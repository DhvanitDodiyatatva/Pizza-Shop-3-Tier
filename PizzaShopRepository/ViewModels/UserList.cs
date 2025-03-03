namespace PizzaShopRepository.ViewModels;

public class UserList
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = null!;
    public bool Status { get; set; }

    public bool IsDeleted { get; set; } = false;
}