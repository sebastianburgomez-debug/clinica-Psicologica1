using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.ClinicalRecord
{
    public class UpdateDiagnosisStatusDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public Guid ClinicalRecordId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        public DiagnosisStatus Status { get; set; }
    }
}
