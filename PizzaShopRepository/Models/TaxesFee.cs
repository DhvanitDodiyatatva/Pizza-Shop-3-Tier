using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class TaxesFee
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public decimal Value { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsDefault { get; set; }

    public bool IsDeleted { get; set; }
}
