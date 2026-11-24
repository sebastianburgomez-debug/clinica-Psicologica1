using Psicologia.Web.Data.Enums;

namespace Psicologia.Web.DTOs.Appointment
{
    public class AppointmentDTO
    {
        public Guid Id { get; set; }
        public Guid DoctorProfileId { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public string? ServiceName { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
