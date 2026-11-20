namespace Psicologia.Web.DTOs.Agenda
{
    public class AvailabilityDTO
    {
        public Guid Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
