using Psicologia.Web.Data.Enums;

namespace Psicologia.Web.DTOs.Staff
{
    public class StaffDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDoctor { get; set; }
        public string? SpecialtyName { get; set; }
        public string? ProfessionalLicense { get; set; }
        public ApprovalStatus? DoctorStatus { get; set; }
    }
}
