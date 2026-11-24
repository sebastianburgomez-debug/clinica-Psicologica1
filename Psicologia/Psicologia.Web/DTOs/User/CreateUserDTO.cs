using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.User
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(128, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(64, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Correo")]
        [EmailAddress(ErrorMessage = "El campo {0} debe ser un Email válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MinLength(4, ErrorMessage = "El campo {0} debe tener por lo menos {1} caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Rol")]
        public Guid RoleId { get; set; }

        public Data.Entities.Role? Role { get; set; }

        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
