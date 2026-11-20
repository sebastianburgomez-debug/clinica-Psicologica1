namespace Psicologia.Web.Core.Extensions
{
    public static class DateExtensions
    {
        // Días de la semana en el orden en que se muestran en pantalla (lunes primero)
        public static readonly DayOfWeek[] WeekDays =
        [
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        ];

        public static string ToSpanish(this DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miércoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sábado",
                _ => "Domingo"
            };
        }

        // Lunes de la semana a la que pertenece la fecha
        public static DateTime StartOfWeek(this DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.Date.AddDays(-diff);
        }

        public static int TotalMinutes(TimeOnly start, TimeOnly end)
        {
            return (int)(end - start).TotalMinutes;
        }
    }
}
