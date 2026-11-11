
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Data.Entities
{
    public class Specialty
    {
        public int SpecialtyId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Relación con UsuarioEspecialidad
        public ICollection<UsuarioEspecialidad> UsuarioEspecialidades
        { get; set; } = new List<UsuarioEspecialidad>();
    }
}
