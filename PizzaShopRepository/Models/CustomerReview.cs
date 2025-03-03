using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class CustomerReview
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? FoodRating { get; set; }

    public int? ServiceRating { get; set; }

    public int? AmbienceRating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order? Order { get; set; }
}
