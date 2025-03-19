using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class Section
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();

    public virtual ICollection<WaitingToken> WaitingTokens { get; set; } = new List<WaitingToken>();
}
