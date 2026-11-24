using Psicologia.Web.Data.Entities;

namespace Psicologia.Web.Core
{
    // Cálculos informativos de ocupación de agenda (no contables)
    public static class OccupancyHelper
    {
        // Minutos disponibles entre dos fechas (inclusive) según los bloques semanales de disponibilidad
        public static int AvailableMinutes(IEnumerable<DoctorAvailability> blocks, DateTime from, DateTime to)
        {
            Dictionary<DayOfWeek, int> minutesByDay = blocks.GroupBy(b => b.DayOfWeek)
                                                            .ToDictionary(g => g.Key, g => g.Sum(b => (int)(b.EndTime - b.StartTime).TotalMinutes));
            int total = 0;

            for (DateTime day = from.Date; day <= to.Date; day = day.AddDays(1))
            {
                if (minutesByDay.TryGetValue(day.DayOfWeek, out int minutes))
                {
                    total += minutes;
                }
            }

            return total;
        }

        public static int WeeklyMinutes(IEnumerable<DoctorAvailability> blocks)
        {
            return blocks.Sum(b => (int)(b.EndTime - b.StartTime).TotalMinutes);
        }

        public static decimal Percent(int booked, int available)
        {
            if (available <= 0)
            {
                return 0;
            }

            return Math.Round(booked * 100m / available, 1);
        }
    }
}
