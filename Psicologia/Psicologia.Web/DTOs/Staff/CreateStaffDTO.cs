using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Staff
{
    public class CreateStaffDTO : IValidatableObject
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Tipo de cuenta")]
        public StaffType StaffType { get; set; } = StaffType.Doctor;

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(128, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(64, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [EmailAddress(ErrorMessage = "El campo {0} debe ser un Email válido")]
        [Display(Name = "Correo")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MinLength(4, ErrorMessage = "El campo {0} debe tener por lo menos {1} caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        // Solo para doctores
        [Display(Name = "Especialidad")]
        public Guid? SpecialtyId { get; set; }

        [MaxLength(32, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Tarjeta profesional")]
        public string? ProfessionalLicense { get; set; }

        [MaxLength(512, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Perfil profesional")]
        public string? Biography { get; set; }

        public IEnumerable<SelectListItem>? Specialties { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StaffType != StaffType.Doctor)
            {
                yield break;
            }

            if (SpecialtyId is null || SpecialtyId == Guid.Empty)
            {
                yield return new ValidationResult("La especialidad es requerida para los doctores", [nameof(SpecialtyId)]);
            }

            if (string.IsNullOrWhiteSpace(ProfessionalLicense))
            {
                yield return new ValidationResult("La tarjeta profesional es requerida para los doctores", [nameof(ProfessionalLicense)]);
            }
        }
    }
}
