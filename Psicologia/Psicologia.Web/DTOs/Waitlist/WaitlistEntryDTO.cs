using Psicologia.Web.Data.Enums;

namespace Psicologia.Web.DTOs.Waitlist
{
    public class WaitlistEntryDTO
    {
        public Guid Id { get; set; }
        public string? PatientName { get; set; }
        public string? ServiceName { get; set; }
        public string? DoctorName { get; set; }
        public DateOnly? PreferredDate { get; set; }
        public string? Notes { get; set; }
        public WaitlistStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
