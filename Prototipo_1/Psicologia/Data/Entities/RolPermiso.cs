
namespace Psicologia.Data.Entities
{
    public class RolPermiso
    {
        // Clave foránea del rol
        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;

        // Clave foránea del permiso
        public int PermisoId { get; set; }

        public Permiso Permiso { get; set; } = null!;
    }
}
