using Psicologia.Web.Data.Abstractions;
using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    public class Appointment : IId
    {
        [Key]
        public Guid Id { get; set; }

        public required Guid DoctorProfileId { get; set; }

        public DoctorProfile? DoctorProfile { get; set; }

        [Required]
        [MaxLength(450)]
        public required string PatientId { get; set; }

        public User? Patient { get; set; }

        public required Guid ClinicalServiceId { get; set; }

        public ClinicalService? ClinicalService { get; set; }

        // Hora local de la clínica
        public DateTime StartsAt { get; set; }

        public DateTime EndsAt { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        [MaxLength(512)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        [MaxLength(256)]
        public string? CreatedBy { get; set; }
    }
}
