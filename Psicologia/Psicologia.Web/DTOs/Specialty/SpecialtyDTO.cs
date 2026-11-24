using Psicologia.Web.Data.Enums;

namespace Psicologia.Web.DTOs.Specialty
{
    public class SpecialtyDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public ApprovalStatus Status { get; set; }
        public string? RequestedBy { get; set; }
    }
}
