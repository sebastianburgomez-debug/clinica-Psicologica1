
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Data.Entities
{
    public class Seccion
    {
        [Key]public int IdSeccion { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // Clave foránea del módulo
        public int IdModulo { get; set; }

        // Relación con Modulo
        public Modulo Modulo { get; set; } = null!;

        // Relación con RolSeccion
        public ICollection<RolSeccion> RolSecciones { get; set; }
            = new List<RolSeccion>();
    }
}
