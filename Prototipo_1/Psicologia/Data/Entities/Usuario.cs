
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Data.Entities
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        // Contraseña almacenada mediante hash
        public string? PasswordHash { get; set; }

        public int RoleId { get; set; }

        // Relación con Role
        public Role Role { get; set; } = null!;

        // Relación con UsuarioEspecialidad
        public ICollection<UsuarioEspecialidad> UsuarioEspecialidades
        { get; set; } = new List<UsuarioEspecialidad>();
    }
}
