using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Appointment
{
    public class CreateAppointmentDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Doctor")]
        public Guid? DoctorProfileId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Paciente")]
        public string? PatientId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Servicio")]
        public Guid? ClinicalServiceId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Fecha")]
        public DateOnly? Date { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Hora")]
        public TimeOnly? StartTime { get; set; }

        [MaxLength(512, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Observaciones")]
        public string? Notes { get; set; }

        public IEnumerable<SelectListItem>? Doctors { get; set; }
        public IEnumerable<SelectListItem>? Patients { get; set; }
        public IEnumerable<SelectListItem>? Services { get; set; }
    }
}
