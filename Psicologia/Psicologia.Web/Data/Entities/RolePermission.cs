namespace Psicologia.Web.Data.Entities
{
    // Tabla intermedia (N:N) entre Role y Permission
    public class RolePermission
    {
        public required Guid RoleId { get; set; }
        public required Guid PermissionId { get; set; }
        public Role? Role { get; set; }
        public Permission? Permission { get; set; }
    }
}
