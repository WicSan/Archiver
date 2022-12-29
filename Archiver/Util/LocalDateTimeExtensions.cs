using NodaTime;

namespace Archiver.Util
{
    public static class LocalDateTimeExtensions
    {
        public static LocalDateTime MinusDays(this LocalDateTime dateTime, int days)
        {
            return dateTime.Minus(Period.FromDays(days));
        }
    }
}
