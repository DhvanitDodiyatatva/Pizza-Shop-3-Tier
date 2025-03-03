namespace PizzaShop.ViewModels;

public class Pagination
{
    public required List<UserList> Users { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

}
