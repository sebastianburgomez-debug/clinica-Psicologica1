using System.ComponentModel.DataAnnotations;

namespace Psicologia.Data.Entities
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Contraseña { get; set; } = string.Empty;

        public string idRole { get; set; } = string.Empty;

    }
}

