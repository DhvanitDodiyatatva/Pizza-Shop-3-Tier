using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class Table
{
    public int Id { get; set; }

    public int? SectionId { get; set; }

    public string Name { get; set; } = null!;

    public int Capacity { get; set; }

    public string? Status { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Section? Section { get; set; }
}
