using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Role
{
    public class UpdateRoleDTO
    {//necesita id para actualizar un rol, por eso se agrega la propiedad id
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Rol")]
        public string Name { get; set; } = string.Empty;

        public List<PermissionsForRoleDTO>? Permissions { get; set; }
        public string? PermissionIds { get; set; } // Ej ["548214fd-4fa6-4d8d-be96-724f1fe36d82","dcbf682d-fb2c-44bf-9969-63bb212845ba"]
    }
}
