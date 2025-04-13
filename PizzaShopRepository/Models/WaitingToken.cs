using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class WaitingToken
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public int NumOfPersons { get; set; }

    public int? SectionId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Section? Section { get; set; }
}
