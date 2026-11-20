using Psicologia.Web.Data.Enums;

namespace Psicologia.Web.Core.Extensions
{
    // Clases CSS (Bootstrap) para los badges de estado en las vistas
    public static class StatusBadgeExtensions
    {
        public static string GetBadgeClass(this ApprovalStatus status)
        {
            return status switch
            {
                ApprovalStatus.Approved => "bg-success",
                ApprovalStatus.Pending => "bg-warning text-dark",
                _ => "bg-danger"
            };
        }

        public static string GetBadgeClass(this AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Scheduled => "bg-primary",
                AppointmentStatus.Attended => "bg-success",
                AppointmentStatus.Cancelled => "bg-secondary",
                _ => "bg-danger"
            };
        }

        public static string GetBadgeClass(this DiagnosisStatus status)
        {
            return status switch
            {
                DiagnosisStatus.Presumptive => "bg-warning text-dark",
                DiagnosisStatus.Confirmed => "bg-danger",
                DiagnosisStatus.Resolved => "bg-success",
                _ => "bg-secondary"
            };
        }
    }
}
