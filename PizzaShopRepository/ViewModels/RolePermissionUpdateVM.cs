namespace PizzaShopRepository.ViewModels;

public class RolePermissionUpdateModel
    {
        public int RoleId { get; set; }
        public List<PermissionUpdateVM> Permissions { get; set; } = new List<PermissionUpdateVM>();
    }
