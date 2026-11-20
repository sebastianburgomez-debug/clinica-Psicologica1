namespace Psicologia.Web.DTOs.Dashboard
{
    // Indicadores meramente informativos del Panel de Dirección
    public class DashboardDTO
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        // Citas
        public int AppointmentsToday { get; set; }
        public int AppointmentsInRange { get; set; }
        public int ScheduledInRange { get; set; }
        public int AttendedInRange { get; set; }
        public int CancelledInRange { get; set; }
        public int NoShowInRange { get; set; }

        // Ocupación: minutos agendados / minutos disponibles en la agenda de los doctores
        public int AvailableMinutes { get; set; }
        public int BookedMinutes { get; set; }
        public decimal OccupancyPercent { get; set; }

        // Indicadores generales
        public int ActiveDoctors { get; set; }
        public int ActiveSecretaries { get; set; }
        public int ActivePatients { get; set; }
        public int ClinicalRecords { get; set; }
        public int ActiveServices { get; set; }
        public int PendingDoctors { get; set; }
        public int PendingSpecialties { get; set; }

        public List<LabelValueDTO> DailyAppointments { get; set; } = [];
        public List<LabelValueDTO> AppointmentsBySpecialty { get; set; } = [];
        public List<DoctorOccupancyDTO> DoctorsOccupancy { get; set; } = [];
    }

    public class LabelValueDTO
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class DoctorOccupancyDTO
    {
        public string DoctorName { get; set; } = string.Empty;
        public string? SpecialtyName { get; set; }
        public int Appointments { get; set; }
        public int Attended { get; set; }
        public int AvailableMinutes { get; set; }
        public int BookedMinutes { get; set; }
        public decimal OccupancyPercent { get; set; }
    }
}
