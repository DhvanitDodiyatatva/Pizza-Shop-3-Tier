namespace PizzaShopRepository.ViewModels;

public class CustomerReviewViewModel
{
    public int OrderId { get; set; }
    public int FoodRating { get; set; }
    public int ServiceRating { get; set; }
    public int AmbienceRating { get; set; }
    public string Comment { get; set; }
}