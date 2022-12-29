using NodaTime;
using NodaTime.Text;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchivePlanner.Planning.Database
{
    public class LocalTimeConverter: JsonConverter<LocalTime>
    {
        private static string Pattern = "HH:mm";

        public override LocalTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                LocalTimePattern.CreateWithInvariantCulture(Pattern).Parse(reader.GetString()!).Value;

        public override void Write(
            Utf8JsonWriter writer,
            LocalTime timeValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(timeValue.ToString(
                    Pattern, CultureInfo.InvariantCulture));
    }
}
