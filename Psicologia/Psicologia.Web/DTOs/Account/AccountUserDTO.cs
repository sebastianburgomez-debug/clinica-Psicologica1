using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Account
{
    public class AccountUserDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(128, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Correo")]
        public string? Email { get; set; }

        [Display(Name = "Rol")]
        public string? RoleName { get; set; }
    }
}
