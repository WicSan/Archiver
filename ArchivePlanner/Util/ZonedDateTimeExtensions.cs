using NodaTime;

namespace ArchivePlanner.Util
{
    public static class ZonedDateTimeExtensions
    {
        public static ZonedDateTime MinusDays(this ZonedDateTime dateTime, int days)
        {
            return dateTime.Minus(Duration.FromDays(days));
        }
    }
}
