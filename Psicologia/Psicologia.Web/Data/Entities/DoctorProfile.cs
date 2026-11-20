using Psicologia.Web.Data.Abstractions;
using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    // Datos profesionales del doctor (1:1 con User)
    public class DoctorProfile : IId
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(450)]
        public required string UserId { get; set; }

        public User? User { get; set; }

        public required Guid SpecialtyId { get; set; }

        public Specialty? Specialty { get; set; }

        // Tarjeta profesional / registro
        [Required]
        [MaxLength(32)]
        public required string ProfessionalLicense { get; set; }

        [MaxLength(512)]
        public string? Biography { get; set; }

        // Flujo de aprobación (Panel de Dirección)
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [MaxLength(256)]
        public string? RequestedBy { get; set; }

        public DateTime? RequestedAt { get; set; }

        [MaxLength(256)]
        public string? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(512)]
        public string? ReviewNotes { get; set; }

        public IEnumerable<DoctorAvailability>? Availabilities { get; set; }

        public IEnumerable<Appointment>? Appointments { get; set; }
    }
}
