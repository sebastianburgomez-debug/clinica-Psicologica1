using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Portal
{
    public class RescheduleAppointmentDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Nueva fecha")]
        public DateOnly? Date { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Nueva hora")]
        public TimeOnly? StartTime { get; set; }

        // Solo se usan para mostrar la cita y consultar los horarios libres; el servicio no confía en ellos
        public Guid DoctorProfileId { get; set; }
        public Guid ClinicalServiceId { get; set; }
        public string? DoctorName { get; set; }
        public string? ServiceName { get; set; }
        public DateTime CurrentStartsAt { get; set; }
    }
}
