using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Enums
{
    // Approved = 0 a propósito: al agregar la columna "Status" a la tabla Specialties
    // los registros que ya existen quedan aprobados con el valor por defecto (0).
    public enum ApprovalStatus
    {
        [Display(Name = "Aprobado")]
        Approved = 0,

        [Display(Name = "Pendiente")]
        Pending = 1,

        [Display(Name = "Rechazado")]
        Rejected = 2
    }
}
