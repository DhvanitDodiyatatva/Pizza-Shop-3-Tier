using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneNo { get; set; }

    public int? NoOfPersons { get; set; }

    public DateOnly? Date { get; set; }

    public int? TotalOrders { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
