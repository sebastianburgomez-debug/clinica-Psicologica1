
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Data.Entities
{
    public class Role
    {
        public int RoleId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Relación con Usuario
        public ICollection<Usuario> Usuarios { get; set; }
            = new List<Usuario>();

        // Relación con RolPermiso
        public ICollection<RolPermiso> RolPermisos { get; set; }
            = new List<RolPermiso>();

        // Relación con RolSeccion
        public ICollection<RolSeccion> RolSecciones { get; set; }
            = new List<RolSeccion>();
    }
}
