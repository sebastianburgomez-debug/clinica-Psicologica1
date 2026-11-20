using Psicologia.Web.Data.Enums;

namespace Psicologia.Web.DTOs.ClinicalRecord
{
    public class DiagnosisDTO
    {
        public Guid Id { get; set; }
        public Guid ClinicalRecordId { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Observations { get; set; }
        public DiagnosisStatus Status { get; set; }
        public DateTime DiagnosedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? DoctorName { get; set; }
        public bool CanEdit { get; set; }
    }
}
