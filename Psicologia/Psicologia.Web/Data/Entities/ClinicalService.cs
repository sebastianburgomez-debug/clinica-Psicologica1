using Psicologia.Web.Data.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    // Catálogo de servicios psicológicos ofrecidos por la clínica
    public class ClinicalService : IId
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public int DurationMinutes { get; set; } = 50;

        // Tarifa de referencia: dato meramente informativo, no se usa para facturar
        public decimal ReferenceRate { get; set; }

        public Guid? SpecialtyId { get; set; }

        public Specialty? Specialty { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
