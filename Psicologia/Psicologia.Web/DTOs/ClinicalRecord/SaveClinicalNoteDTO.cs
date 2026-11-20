using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.ClinicalRecord
{
    // Se usa para crear (Id vacío) y para editar (Id con valor) una nota clínica
    public class SaveClinicalNoteDTO
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public Guid ClinicalRecordId { get; set; }

        [Display(Name = "Paciente")]
        public string? PatientName { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Tipo de nota")]
        public ClinicalNoteType Type { get; set; } = ClinicalNoteType.Session;

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Fecha de la sesión")]
        public DateTime SessionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(4000, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Contenido")]
        public string Content { get; set; } = string.Empty;
    }
}
