using Psicologia.Web.Data.Abstractions;
using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    public class Specialty : IId
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Flujo de aprobación (Panel de Dirección)
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Approved;

        [MaxLength(256)]
        public string? RequestedBy { get; set; }

        public DateTime? RequestedAt { get; set; }

        [MaxLength(256)]
        public string? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(512)]
        public string? ReviewNotes { get; set; }

        public IEnumerable<DoctorProfile>? DoctorProfiles { get; set; }

        public IEnumerable<ClinicalService>? ClinicalServices { get; set; }
    }
}
