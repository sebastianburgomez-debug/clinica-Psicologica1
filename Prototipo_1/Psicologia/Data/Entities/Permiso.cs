
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Data.Entities
{
    public class Permiso
    {
        [Key]
        public int IdPermiso { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // Relación con RolPermiso
        public ICollection<RolPermiso> RolPermisos { get; set; }
            = new List<RolPermiso>();
    }
}
