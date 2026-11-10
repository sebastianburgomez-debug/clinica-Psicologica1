using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Role
{
    public class PermissionDTO
    {
        public Guid Id { get; set; }

        [Display(Name = "Código")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Permiso")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Módulo")]
        public string Module { get; set; } = string.Empty;
    }
}
