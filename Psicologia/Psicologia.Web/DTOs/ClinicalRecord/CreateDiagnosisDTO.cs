using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.ClinicalRecord
{
    public class CreateDiagnosisDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public Guid ClinicalRecordId { get; set; }

        [Display(Name = "Paciente")]
        public string? PatientName { get; set; }

        [MaxLength(16, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Código (CIE-10 / DSM-5)")]
        public string? Code { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(256, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Diagnóstico")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1024, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Observaciones")]
        public string? Observations { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Estado")]
        public DiagnosisStatus Status { get; set; } = DiagnosisStatus.Presumptive;

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Fecha")]
        public DateTime DiagnosedAt { get; set; } = DateTime.Today;
    }
}
