using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.User
{
    public class UpdateUserDTO
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(128, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(64, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Correo")]
        [EmailAddress(ErrorMessage = "El campo {0} debe ser un Email válido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Rol")]
        public Guid RoleId { get; set; }

        public Data.Entities.Role? Role { get; set; }

        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
