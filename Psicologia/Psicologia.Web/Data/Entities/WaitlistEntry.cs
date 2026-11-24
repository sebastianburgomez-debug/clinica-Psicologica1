using Psicologia.Web.Data.Abstractions;
using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    // Paciente en lista de espera cuando no hay disponibilidad inmediata (la gestiona la secretaria)
    public class WaitlistEntry : IId
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(450)]
        public required string PatientId { get; set; }
        public User? Patient { get; set; }

        public required Guid ClinicalServiceId { get; set; }
        public ClinicalService? ClinicalService { get; set; }

        // Doctor preferido (opcional)
        public Guid? DoctorProfileId { get; set; }
        public DoctorProfile? DoctorProfile { get; set; }

        // Fecha en la que el paciente preferiría la cita (opcional)
        public DateOnly? PreferredDate { get; set; }

        [MaxLength(512)]
        public string? Notes { get; set; }

        public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;

        // Cita que se creó al asignar el cupo (sin navegación a propósito)
        public Guid? AssignedAppointmentId { get; set; }

        // UTC
        public DateTime CreatedAt { get; set; }

        [MaxLength(256)]
        public string? CreatedBy { get; set; }
    }
}
