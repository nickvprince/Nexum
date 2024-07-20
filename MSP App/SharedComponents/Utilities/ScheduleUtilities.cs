using SharedComponents.Entities.DbEntities;

namespace SharedComponents.Utilities
{
    public class ScheduleUtilities
    {
        public static string ConvertToTenantServerFormat(DeviceJobSchedule schedule)
        {
            return $"{(schedule.Sunday ? '1' : '0')}" +
                $"{(schedule.Monday ? '1' : '0')}" +
                $"{(schedule.Tuesday ? '1' : '0')}" +
                $"{(schedule.Wednesday ? '1' : '0')}" +
                $"{(schedule.Thursday ? '1' : '0')}" +
                $"{(schedule.Friday ? '1' : '0')}" +
                $"{(schedule.Saturday ? '1' : '0')}";
        }

        public static DeviceJobSchedule ConvertFromTenantServerFormat(string schedule)
        {
            if (schedule.Length != 7)
            {
                throw new ArgumentException("Schedule string must be 7 characters long.");
            }

            return new DeviceJobSchedule
            {
                Sunday = schedule[0] == '1',
                Monday = schedule[1] == '1',
                Tuesday = schedule[2] == '1',
                Wednesday = schedule[3] == '1',
                Thursday = schedule[4] == '1',
                Friday = schedule[5] == '1',
                Saturday = schedule[6] == '1'
            };
        }
    }
}
