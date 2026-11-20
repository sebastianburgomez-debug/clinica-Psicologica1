using Psicologia.Web.Data.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    // Trazabilidad de inicios de sesión y modificaciones críticas
    public class AuditLog : IId
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(256)]
        public string? UserName { get; set; }

        [Required]
        [MaxLength(32)]
        public required string Action { get; set; }

        [Required]
        [MaxLength(64)]
        public required string Entity { get; set; }

        [MaxLength(64)]
        public string? EntityId { get; set; }

        [Required]
        [MaxLength(512)]
        public required string Description { get; set; }

        [MaxLength(64)]
        public string? IpAddress { get; set; }

        // UTC
        public DateTime CreatedAt { get; set; }
    }
}
