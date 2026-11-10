using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Specialty
{
    public class CreateSpecialtyDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Especialidad")]
        public string Name { get; set; } = string.Empty;
    }
}
