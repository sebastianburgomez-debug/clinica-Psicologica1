using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Enums
{
    public enum AppointmentStatus
    {
        [Display(Name = "Programada")]
        Scheduled = 0,

        [Display(Name = "Atendida")]
        Attended = 1,

        [Display(Name = "Cancelada")]
        Cancelled = 2,

        [Display(Name = "No asistió")]
        NoShow = 3
    }
}
