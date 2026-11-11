using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    // Id, Email y Password (PasswordHash) los provee IdentityUser
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(128)]
        public required string Name { get; set; }

        public required Guid RoleId { get; set; }

        public Role? Role { get; set; }
    }
}
