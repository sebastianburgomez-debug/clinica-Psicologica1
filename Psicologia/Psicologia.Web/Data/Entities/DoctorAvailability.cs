using Psicologia.Web.Data.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    // Bloque semanal de disponibilidad horaria de un doctor (agenda personal)
    public class DoctorAvailability : IId
    {
        [Key]
        public Guid Id { get; set; }

        public required Guid DoctorProfileId { get; set; }

        public DoctorProfile? DoctorProfile { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}
