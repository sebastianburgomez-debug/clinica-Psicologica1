using Psicologia.Web.Data.Enums;

namespace Psicologia.Web.DTOs.Approval
{
    public class SpecialtyApprovalDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ApprovalStatus Status { get; set; }
        public string? RequestedBy { get; set; }
        public DateTime? RequestedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
    }
}
