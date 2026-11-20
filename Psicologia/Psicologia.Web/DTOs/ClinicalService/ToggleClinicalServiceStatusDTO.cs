using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.ClinicalService
{
    public class ToggleClinicalServiceStatusDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid ClinicalServiceId { get; set; }

        public bool Deactivate { get; set; } = true;
    }
}
