using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Waitlist
{
    // La secretaria elige doctor, fecha y hora del cupo disponible para el paciente en espera
    public class AssignWaitlistEntryDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Doctor")]
        public Guid? DoctorProfileId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Fecha")]
        public DateOnly? Date { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Hora")]
        public TimeOnly? StartTime { get; set; }

        // Solo para mostrar
        public string? PatientName { get; set; }
        public string? ServiceName { get; set; }
        public IEnumerable<SelectListItem>? Doctors { get; set; }
    }
}
