using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class OrderItemModifier
{
    public int OrderItemId { get; set; }

    public int ModifierId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual Modifier Modifier { get; set; } = null!;

    public virtual OrderItem OrderItem { get; set; } = null!;
}
