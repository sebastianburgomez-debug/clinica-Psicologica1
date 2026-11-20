using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Staff
{
    // Tipos de cuenta que se gestionan desde el módulo de Personal
    public enum StaffType
    {
        [Display(Name = "Doctor")]
        Doctor = 0,

        [Display(Name = "Secretaria")]
        Secretary = 1
    }
}
