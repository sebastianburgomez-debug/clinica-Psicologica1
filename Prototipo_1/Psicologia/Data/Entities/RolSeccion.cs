
namespace Psicologia.Data.Entities
{
    public class RolSeccion
    {
        // Clave foránea del rol
        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;

        // Clave foránea de la sección
        public int SeccionId { get; set; }

        public Seccion Seccion { get; set; } = null!;
    }
}
