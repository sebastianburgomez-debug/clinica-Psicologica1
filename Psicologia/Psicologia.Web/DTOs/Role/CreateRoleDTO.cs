using System.ComponentModel.DataAnnotations;
//transportar datos entre el servidor y el cliente
//
namespace Psicologia.Web.DTOs.Role
{ //al crear un nuveo rol id no existe, se genera guid 
    public class CreateRoleDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Rol")]
        public string Name { get; set; } = string.Empty;

        public List<PermissionsForRoleDTO>? Permissions { get; set; }
        public string? PermissionIds { get; set; } // Ej ["548214fd-4fa6-4d8d-be96-724f1fe36d82","dcbf682d-fb2c-44bf-9969-63bb212845ba"]
    }
}
