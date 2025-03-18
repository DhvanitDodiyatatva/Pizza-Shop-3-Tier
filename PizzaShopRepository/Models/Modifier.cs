using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class Modifier
{
    public int Id { get; set; }

    public int? ModifierGroupId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Unit { get; set; }

    public int? Quantity { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ModifierGroup? ModifierGroup { get; set; }

    public virtual ICollection<OrderItemModifier> OrderItemModifiers { get; set; } = new List<OrderItemModifier>();
}
