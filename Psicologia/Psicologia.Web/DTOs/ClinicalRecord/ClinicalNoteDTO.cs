using Psicologia.Web.Data.Enums;

namespace Psicologia.Web.DTOs.ClinicalRecord
{
    public class ClinicalNoteDTO
    {
        public Guid Id { get; set; }
        public Guid ClinicalRecordId { get; set; }
        public ClinicalNoteType Type { get; set; }
        public DateTime SessionDate { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid DoctorProfileId { get; set; }
        public string? DoctorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool CanEdit { get; set; }
    }
}
