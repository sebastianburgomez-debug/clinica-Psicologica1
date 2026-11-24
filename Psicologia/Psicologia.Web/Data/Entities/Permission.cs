using Psicologia.Web.Data.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    public class Permission : IId
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(32)]
        public required string Code { get; set; }

        [Required]
        [MaxLength(128)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(32)]
        public required string Module { get; set; }

        public IEnumerable<RolePermission>? RolePermissions { get; set; }
    }
}
