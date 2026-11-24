using Microsoft.AspNetCore.Mvc.Rendering;
using Psicologia.Web.Data.Enums;
using Psicologia.Web.DTOs.Appointment;

namespace Psicologia.Web.DTOs.DoctorPanel
{
    // Resumen meramente informativo de las atenciones del doctor.
    // No contiene liquidaciones, comisiones ni cálculos contables.
    public class DoctorPanelDTO
    {
        public Guid? DoctorProfileId { get; set; }
        public string? DoctorName { get; set; }
        public string? SpecialtyName { get; set; }
        public string? ProfessionalLicense { get; set; }
        public ApprovalStatus? Status { get; set; }

        public bool CanSelectDoctor { get; set; }
        public IEnumerable<SelectListItem>? Doctors { get; set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int TotalAppointments { get; set; }
        public int Attended { get; set; }
        public int Scheduled { get; set; }
        public int Cancelled { get; set; }
        public int NoShow { get; set; }
        public int PatientsAttended { get; set; }
        public int NotesWritten { get; set; }
        public int ActiveDiagnoses { get; set; }

        public int WeeklyAvailableMinutes { get; set; }
        public decimal OccupancyPercent { get; set; }

        public List<DoctorServiceSummaryDTO> Services { get; set; } = [];
        public List<AppointmentDTO> UpcomingAppointments { get; set; } = [];
    }

    public class DoctorServiceSummaryDTO
    {
        public string ServiceName { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }

        // Valor de referencia informativo definido en Políticas
        public decimal ReferenceRate { get; set; }

        public int AttendedInRange { get; set; }
    }
}
