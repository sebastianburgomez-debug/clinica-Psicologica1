using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.DTOs.Policy
{
    public class BusinessHourDTO
    {
        public Guid Id { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        [Display(Name = "Abierto")]
        public bool IsOpen { get; set; }

        [Display(Name = "Apertura")]
        public TimeOnly OpenTime { get; set; }

        [Display(Name = "Cierre")]
        public TimeOnly CloseTime { get; set; }
    }
}
