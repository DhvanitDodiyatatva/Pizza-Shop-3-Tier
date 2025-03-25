using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class ItemModifierGroup
{
    public int ItemId { get; set; }

    public int ModifierGroupId { get; set; }

    public int Id { get; set; }

    public int? MinLoad { get; set; }

    public int? MaxLoad { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual ModifierGroup ModifierGroup { get; set; } = null!;
}
