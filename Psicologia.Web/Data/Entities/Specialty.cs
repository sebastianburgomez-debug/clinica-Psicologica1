using Psicologia.Web.Data.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    public class Specialty : IId
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
