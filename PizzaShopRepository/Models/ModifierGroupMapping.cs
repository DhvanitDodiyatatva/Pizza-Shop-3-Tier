using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class ModifierGroupMapping
{
    public int Id { get; set; }

    public int ModifierGroupId { get; set; }

    public int ModifierId { get; set; }

    public virtual Modifier Modifier { get; set; } = null!;

    public virtual ModifierGroup ModifierGroup { get; set; } = null!;
}
