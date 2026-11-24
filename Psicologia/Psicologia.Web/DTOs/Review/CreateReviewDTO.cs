using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Review
{
    public class CreateReviewDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid AppointmentId { get; set; }

        [Required(ErrorMessage = "Seleccione una calificación")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre {1} y {2}")]
        [Display(Name = "Calificación")]
        public int? Rating { get; set; }

        [MaxLength(1024, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Comentarios")]
        public string? Comment { get; set; }

        // Solo para mostrar la sesión que se califica
        public string? DoctorName { get; set; }
        public string? ServiceName { get; set; }
        public DateTime SessionDate { get; set; }
    }
}
