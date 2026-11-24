using Psicologia.Web.Data.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{//representacion de las tablas
    public class Role : IId
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public IEnumerable<RolePermission>? RolePermissions { get; set; }

        public IEnumerable<User>? Users { get; set; }
    }
}
