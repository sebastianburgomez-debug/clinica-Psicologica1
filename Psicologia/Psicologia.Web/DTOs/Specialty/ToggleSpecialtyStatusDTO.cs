using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Specialty
{
    public class ToggleSpecialtyStatusDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid SpecialtyId { get; set; }

        public bool Deactivate { get; set; } = true;
    }
}
