using Psicologia.Web.Data.Enums;

namespace Psicologia.Web.DTOs.Approval
{
    public class DoctorApprovalDTO
    {
        public Guid DoctorProfileId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool UserIsActive { get; set; }
        public string? SpecialtyName { get; set; }
        public ApprovalStatus SpecialtyStatus { get; set; }
        public string ProfessionalLicense { get; set; } = string.Empty;
        public string? Biography { get; set; }
        public ApprovalStatus Status { get; set; }
        public string? RequestedBy { get; set; }
        public DateTime? RequestedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
    }
}
