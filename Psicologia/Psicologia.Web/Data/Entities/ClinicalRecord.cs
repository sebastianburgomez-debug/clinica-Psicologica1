using Psicologia.Web.Data.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    // Historia clínica: una por paciente
    public class ClinicalRecord : IId
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(450)]
        public required string PatientId { get; set; }

        public User? Patient { get; set; }

        [MaxLength(32)]
        public string? DocumentNumber { get; set; }

        public DateOnly? BirthDate { get; set; }

        [MaxLength(32)]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(1024)]
        public required string ConsultationReason { get; set; }

        [MaxLength(2048)]
        public string? Background { get; set; }

        public DateTime CreatedAt { get; set; }

        [MaxLength(256)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public IEnumerable<ClinicalNote>? Notes { get; set; }

        public IEnumerable<Diagnosis>? Diagnoses { get; set; }
    }
}
