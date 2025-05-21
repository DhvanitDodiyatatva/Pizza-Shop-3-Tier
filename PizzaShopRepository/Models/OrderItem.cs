using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public string? SpecialInstructions { get; set; }

    public string? ItemStatus { get; set; }

    public decimal? CurrentTaxPercentage { get; set; }

    public int ReadyQuantity { get; set; }

    public DateTime? ReadyAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<OrderItemModifier> OrderItemModifiers { get; set; } = new List<OrderItemModifier>();
}
