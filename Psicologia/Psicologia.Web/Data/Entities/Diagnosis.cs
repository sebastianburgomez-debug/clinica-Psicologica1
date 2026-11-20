using Psicologia.Web.Data.Abstractions;
using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    public class Diagnosis : IId
    {
        [Key]
        public Guid Id { get; set; }

        public required Guid ClinicalRecordId { get; set; }

        public ClinicalRecord? ClinicalRecord { get; set; }

        public required Guid DoctorProfileId { get; set; }

        public DoctorProfile? DoctorProfile { get; set; }

        // Código CIE-10 / DSM-5 (opcional)
        [MaxLength(16)]
        public string? Code { get; set; }

        [Required]
        [MaxLength(256)]
        public required string Name { get; set; }

        [MaxLength(1024)]
        public string? Observations { get; set; }

        public DiagnosisStatus Status { get; set; } = DiagnosisStatus.Presumptive;

        public DateTime DiagnosedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
