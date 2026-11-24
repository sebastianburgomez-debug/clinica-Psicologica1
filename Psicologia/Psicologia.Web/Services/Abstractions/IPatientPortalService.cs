using Psicologia.Web.Core;
using Psicologia.Web.DTOs.Appointment;
using Psicologia.Web.DTOs.Portal;

namespace Psicologia.Web.Services.Abstractions
{
    // Portal del paciente: todo opera sobre las citas del paciente de la sesión
    public interface IPatientPortalService
    {
        public Task<Response<List<PatientAppointmentDTO>>> GetMyAppointmentsAsync();
        public Task<Response<AppointmentDTO>> BookAsync(BookAppointmentDTO dto);
        public Task<Response> CancelAsync(Guid appointmentId);
        public Task<Response<RescheduleAppointmentDTO>> GetForRescheduleAsync(Guid appointmentId);
        public Task<Response> RescheduleAsync(RescheduleAppointmentDTO dto);

        // Horas libres (formato HH:mm) de un doctor para un servicio en una fecha
        public Task<List<string>> GetAvailableSlotsAsync(Guid doctorProfileId, Guid clinicalServiceId, DateOnly date);
    }
}
