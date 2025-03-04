namespace PizzaShopRepository.ViewModels;

public class PermissionUpdateVM
    {
        public int PermissionId { get; set; }
        public bool? CanView { get; set; }
        public bool? CanAddEdit { get; set; }
        public bool? CanDelete { get; set; }
    }