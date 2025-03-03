using System;
using System.Collections.Generic;

namespace PizzaShopRepository.Models;

public partial class RolePermission
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public bool? CanView { get; set; }

    public bool? CanAddEdit { get; set; }

    public bool? CanDelete { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
