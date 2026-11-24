using Psicologia.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Appointment
{
    public class ChangeAppointmentStatusDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public AppointmentStatus Status { get; set; }
    }
}
