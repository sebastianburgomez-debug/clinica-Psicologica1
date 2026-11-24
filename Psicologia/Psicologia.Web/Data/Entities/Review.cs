using Psicologia.Web.Data.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    // Reseña del paciente sobre una sesión atendida (una por cita)
    public class Review : IId
    {
        [Key]
        public Guid Id { get; set; }

        public required Guid AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        // Se guarda el doctor para poder filtrar y promediar sin pasar por la cita
        public required Guid DoctorProfileId { get; set; }
        public DoctorProfile? DoctorProfile { get; set; }

        [Required]
        [MaxLength(450)]
        public required string PatientId { get; set; }
        public User? Patient { get; set; }

        // Calificación de 1 a 5
        public int Rating { get; set; }

        [MaxLength(1024)]
        public string? Comment { get; set; }

        // UTC
        public DateTime CreatedAt { get; set; }
    }
}
