using Psicologia.Web.Data.Enums;

namespace Psicologia.Web.DTOs.Portal
{
    // Cita vista desde el portal del paciente, con las acciones que puede hacer sobre ella
    public class PatientAppointmentDTO
    {
        public Guid Id { get; set; }
        public string? DoctorName { get; set; }
        public string? ServiceName { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Notes { get; set; }

        public bool HasReview { get; set; }

        // Las calcula el servicio según el estado de la cita y la anticipación mínima
        public bool CanCancel { get; set; }
        public bool CanReschedule { get; set; }
        public bool CanReview { get; set; }
    }
}
