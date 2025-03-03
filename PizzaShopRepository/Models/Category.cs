using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
