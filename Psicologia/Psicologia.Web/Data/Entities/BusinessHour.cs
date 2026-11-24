using Psicologia.Web.Data.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Psicologia.Web.Data.Entities
{
    // Horario general de atención de la clínica (un registro por día de la semana)
    public class BusinessHour : IId
    {
        [Key]
        public Guid Id { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public bool IsOpen { get; set; }

        public TimeOnly OpenTime { get; set; }

        public TimeOnly CloseTime { get; set; }
    }
}
