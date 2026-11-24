using Psicologia.Web.Data.Abstractions;
using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    // Nota de sesión o de evolución dentro de una historia clínica
    public class ClinicalNote : IId
    {
        [Key]
        public Guid Id { get; set; }

        public required Guid ClinicalRecordId { get; set; }

        public ClinicalRecord? ClinicalRecord { get; set; }

        public required Guid DoctorProfileId { get; set; }

        public DoctorProfile? DoctorProfile { get; set; }

        public ClinicalNoteType Type { get; set; } = ClinicalNoteType.Session;

        public DateTime SessionDate { get; set; }

        [Required]
        [MaxLength(4000)]
        public required string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
