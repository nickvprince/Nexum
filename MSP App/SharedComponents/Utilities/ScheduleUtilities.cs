using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Utilities
{
    public class ScheduleUtilities
    {
        public static string ConvertToTenantServerFormat(DayOfWeek[] days)
        {
            if (days.Length != 7)
            {
                throw new ArgumentException("Schedule string must be 7 characters long.");
            }
            char[] schedule = new char[] { '0', '0', '0', '0', '0', '0', '0' };
            foreach (var day in days)
            {
                schedule[(int)day] = '1';
            }
            return new string(schedule);
        }

        public static DayOfWeek[] ConvertFromTenantServerFormat(string schedule)
        {
            if (schedule.Length != 7)
            {
                throw new ArgumentException("Schedule string must be 7 characters long.");
            }

            return schedule
                .Select((c, index) => new { c, index })
                .Where(x => x.c == '1')
                .Select(x => (DayOfWeek)x.index)
                .ToArray();
        }
    }
}
