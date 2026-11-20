using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Staff
{
    public class ToggleStaffStatusDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid UserId { get; set; }

        public bool Deactivate { get; set; } = true;
    }
}
