using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public string? OrderStatus { get; set; }

    public decimal TotalAmount { get; set; }

    public string? PaymentStatus { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? Rating { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<CustomerReview> CustomerReviews { get; set; } = new List<CustomerReview>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
