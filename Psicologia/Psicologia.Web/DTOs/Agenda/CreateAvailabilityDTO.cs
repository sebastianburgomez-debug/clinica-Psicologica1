using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Agenda
{
    public class CreateAvailabilityDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid DoctorProfileId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Día")]
        public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Hora inicio")]
        public TimeOnly StartTime { get; set; } = new TimeOnly(8, 0);

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Hora fin")]
        public TimeOnly EndTime { get; set; } = new TimeOnly(12, 0);
    }
}
