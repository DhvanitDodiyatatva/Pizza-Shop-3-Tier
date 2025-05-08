using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class OrderTax
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? TaxId { get; set; }

    public decimal? TaxPercentage { get; set; }

    public decimal? TaxFlat { get; set; }

    public bool IsApplied { get; set; }

    public virtual Order? Order { get; set; }

    public virtual TaxesFee? Tax { get; set; }
}
