namespace IdentityManagement.WebApp.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public List<RoleItemViewModel> Roles { get; set; } = new();
    }

    public class RoleItemViewModel
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public bool IsSelected { get; set; }
    }
}
