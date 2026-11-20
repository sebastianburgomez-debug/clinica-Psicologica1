namespace Psicologia.Web.DTOs.ClinicalRecord
{
    public class ClinicalRecordDTO
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string? PatientName { get; set; }
        public string? PatientEmail { get; set; }
        public string? DocumentNumber { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Phone { get; set; }
        public string ConsultationReason { get; set; } = string.Empty;
        public string? Background { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int NotesCount { get; set; }
        public DateTime? LastSessionDate { get; set; }

        public List<ClinicalNoteDTO> Notes { get; set; } = [];
        public List<DiagnosisDTO> Diagnoses { get; set; } = [];

        // Permisos calculados para el usuario actual
        public bool CanEditRecord { get; set; }
        public bool CanWriteNotes { get; set; }
    }
}
