namespace Psicologia.Web.DTOs.Review
{
    public class ReviewDTO
    {
        public Guid Id { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public string? ServiceName { get; set; }
        public DateTime SessionDate { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
