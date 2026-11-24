using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Enums
{
    public enum WaitlistStatus
    {
        [Display(Name = "En espera")]
        Waiting = 0,

        [Display(Name = "Asignada")]
        Assigned = 1,

        [Display(Name = "Cancelada")]
        Cancelled = 2
    }
}
