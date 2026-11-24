using Microsoft.AspNetCore.Mvc.Rendering;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Appointment;
using Psicologia.Web.DTOs.Policy;

namespace Psicologia.Web.DTOs.Agenda
{
    public class AgendaDTO
    {
        public Guid? DoctorProfileId { get; set; }
        public string? DoctorName { get; set; }
        public string? SpecialtyName { get; set; }
        public ApprovalStatus? DoctorStatus { get; set; }

        // true cuando quien consulta no es doctor (administrador/secretaria) y puede elegir el doctor
        public bool CanSelectDoctor { get; set; }
        public IEnumerable<SelectListItem>? Doctors { get; set; }

        public DateTime WeekStart { get; set; }
        public int WeeklyAvailableMinutes { get; set; }

        public List<AvailabilityDTO> Availabilities { get; set; } = [];
        public List<BusinessHourDTO> BusinessHours { get; set; } = [];
        public List<AppointmentDTO> WeekAppointments { get; set; } = [];

        // Formulario para agregar un bloque
        public CreateAvailabilityDTO NewAvailability { get; set; } = new();
    }
}
