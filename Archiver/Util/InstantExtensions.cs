﻿using NodaTime;

namespace Archiver.Util
{
    public static class InstantExtensions
    {
        public static LocalDateTime ToLocalDateTime(this Instant instant)
        {
            var systemZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            return instant.InZone(systemZone).LocalDateTime;
        }
    }
}
