
namespace SharedComponents.Utilities
{
    public class DateTimeUtilities
    {
        public static DateTime EstNow()
        {
            TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime utcNow = DateTime.UtcNow;
            DateTime estTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, estZone);
            return estTime;
        }
        public static DateTime ConvertToEst(DateTime utcDateTime)
        {
            TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime estTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, estZone);
            return estTime;
        }
    }
}
