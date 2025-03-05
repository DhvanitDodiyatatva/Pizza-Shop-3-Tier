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
        public bool CanView { get; set; }
        public bool CanAddEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}