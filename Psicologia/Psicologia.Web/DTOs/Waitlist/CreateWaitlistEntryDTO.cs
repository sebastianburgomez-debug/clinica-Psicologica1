using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Waitlist
{
    public class CreateWaitlistEntryDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Paciente")]
        public string? PatientId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Servicio")]
        public Guid? ClinicalServiceId { get; set; }

        [Display(Name = "Doctor preferido (opcional)")]
        public Guid? DoctorProfileId { get; set; }

        [Display(Name = "Fecha preferida (opcional)")]
        public DateOnly? PreferredDate { get; set; }

        [MaxLength(512, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Observaciones")]
        public string? Notes { get; set; }

        public IEnumerable<SelectListItem>? Patients { get; set; }
        public IEnumerable<SelectListItem>? Services { get; set; }
        public IEnumerable<SelectListItem>? Doctors { get; set; }
    }
}
