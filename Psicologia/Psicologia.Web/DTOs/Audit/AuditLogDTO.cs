namespace Psicologia.Web.DTOs.Audit
{
    public class AuditLogDTO
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
