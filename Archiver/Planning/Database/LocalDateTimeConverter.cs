using NodaTime;
using NodaTime.Text;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchivePlanner.Planning.Database
{
    public class LocalDateTimeConverter : JsonConverter<LocalDateTime?>
    {
        public override LocalDateTime? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if(reader.GetString() is null)
                return null;

            return LocalDateTimePattern.GeneralIso.Parse(reader.GetString()!).Value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            LocalDateTime? dateValue,
            JsonSerializerOptions options)
        {
            if(dateValue is null)
                writer.WriteStringValue((string?)null);
            else
                writer.WriteStringValue(LocalDateTimePattern.GeneralIso.Format(dateValue.Value));
        }
    }
}
