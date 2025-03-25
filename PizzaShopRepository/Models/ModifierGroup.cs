using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class ModifierGroup
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<ItemModifierGroup> ItemModifierGroups { get; set; } = new List<ItemModifierGroup>();

    public virtual ICollection<ModifierGroupMapping> ModifierGroupMappings { get; set; } = new List<ModifierGroupMapping>();
}
