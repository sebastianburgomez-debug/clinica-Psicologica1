using Microsoft.AspNetCore.Mvc.Rendering;
using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Staff
{
    public class UpdateStaffDTO : IValidatableObject
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public Guid Id { get; set; }

        // El tipo de cuenta no se puede cambiar una vez creada
        [Display(Name = "Tipo de cuenta")]
        public StaffType StaffType { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [MaxLength(128, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Correo")]
        public string? Email { get; set; }

        [Display(Name = "Especialidad")]
        public Guid? SpecialtyId { get; set; }

        [MaxLength(32, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Tarjeta profesional")]
        public string? ProfessionalLicense { get; set; }

        [MaxLength(512, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres")]
        [Display(Name = "Perfil profesional")]
        public string? Biography { get; set; }

        [Display(Name = "Estado de aprobación")]
        public ApprovalStatus? DoctorStatus { get; set; }

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
