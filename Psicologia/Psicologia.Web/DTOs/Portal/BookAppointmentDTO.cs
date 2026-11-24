using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Portal
{
    // Solicitud de cita del paciente: el paciente NO viene en el DTO, se toma de la sesión
    public class BookAppointmentDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Doctor")]
        public Guid? DoctorProfileId { get; set; }

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
        public IEnumerable<SelectListItem>? Services { get; set; }
    }
}
