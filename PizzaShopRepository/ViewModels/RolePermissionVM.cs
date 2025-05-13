namespace PizzaShopRepository.ViewModels
{
    public class RolePermissionVM
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;

        public List<PermissionDetail> Permissions { get; set; } = new List<PermissionDetail>();
    }

    public class PermissionDetail
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool CanView { get; set; }
        public bool CanAddEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class ChangedPermission
    {
        public int index { get; set; }
        public bool isSelected { get; set; }
        public bool canView { get; set; }
        public bool canAddEdit { get; set; }
        public bool canDelete { get; set; }
    }
}