using System.ComponentModel.DataAnnotations;

namespace Psicologia.Data.Entities
{
    public class Modulo
    {
        [Key]
        public int IdModulo { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Seccion> Secciones { get; set; }
            = new List<Seccion>();
    }
}