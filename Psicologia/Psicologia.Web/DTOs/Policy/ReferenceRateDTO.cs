using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Policy
{
    // Tarifa de referencia de un servicio: valor meramente informativo
    public class ReferenceRateDTO
    {
        public Guid ClinicalServiceId { get; set; }

        // Solo lectura (no se envía en el formulario): nullable para que no sea requerido implícitamente
        public string? ServiceName { get; set; }

        public string? SpecialtyName { get; set; }

        public int DurationMinutes { get; set; }

        public bool IsActive { get; set; }

        [Range(typeof(decimal), "0", "100000000", ErrorMessage = "El campo {0} debe estar entre {1} y {2}")]
        [Display(Name = "Tarifa de referencia")]
        public decimal ReferenceRate { get; set; }
    }
}
