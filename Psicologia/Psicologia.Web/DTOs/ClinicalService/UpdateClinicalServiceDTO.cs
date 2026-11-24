using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.ClinicalService
{
    public class UpdateClinicalServiceDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Servicio")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(256, ErrorMessage = "El campo {0} debe tener como máximo {1} caracteres.")]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Range(15, 240, ErrorMessage = "El campo {0} debe estar entre {1} y {2} minutos.")]
        [Display(Name = "Duración (minutos)")]
        public int DurationMinutes { get; set; } = 50;

        [Range(typeof(decimal), "0", "100000000", ErrorMessage = "El campo {0} debe estar entre {1} y {2}.")]
        [Display(Name = "Tarifa de referencia")]
        public decimal ReferenceRate { get; set; }

        [Display(Name = "Especialidad")]
        public Guid? SpecialtyId { get; set; }

        public IEnumerable<SelectListItem>? Specialties { get; set; }
    }
}
