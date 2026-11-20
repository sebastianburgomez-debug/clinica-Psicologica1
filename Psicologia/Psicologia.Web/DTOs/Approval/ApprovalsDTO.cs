namespace Psicologia.Web.DTOs.Approval
{
    public class ApprovalsDTO
    {
        public List<DoctorApprovalDTO> PendingDoctors { get; set; } = [];
        public List<SpecialtyApprovalDTO> PendingSpecialties { get; set; } = [];

        // Últimas revisiones realizadas (historial)
        public List<DoctorApprovalDTO> ReviewedDoctors { get; set; } = [];
        public List<SpecialtyApprovalDTO> ReviewedSpecialties { get; set; } = [];
    }
}
