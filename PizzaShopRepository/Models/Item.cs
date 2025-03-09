using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class Item
{
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string ItemType { get; set; } = null!;

    public int? Quantity { get; set; }

    public string? Unit { get; set; }

    public bool IsAvailable { get; set; }

    public string? ShortCode { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ModifierGroup> ModifierGroups { get; set; } = new List<ModifierGroup>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
