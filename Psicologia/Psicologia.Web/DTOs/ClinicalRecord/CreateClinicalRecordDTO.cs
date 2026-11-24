using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.ClinicalRecord
{
    public class CreateClinicalRecordDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Paciente")]
        public string? PatientId { get; set; }

        [MaxLength(32, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Documento")]
        public string? DocumentNumber { get; set; }

        [Display(Name = "Fecha de nacimiento")]
        public DateOnly? BirthDate { get; set; }

        [MaxLength(32, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(1024, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Motivo de consulta")]
        public string ConsultationReason { get; set; } = string.Empty;

        [MaxLength(2048, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Antecedentes")]
        public string? Background { get; set; }

        public IEnumerable<SelectListItem>? Patients { get; set; }
    }
}
