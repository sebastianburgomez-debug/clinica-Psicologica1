using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Approval
{
    public class ReviewRequestDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }

        public bool Approve { get; set; }

        [MaxLength(512, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Observaciones")]
        public string? Notes { get; set; }
    }
}
