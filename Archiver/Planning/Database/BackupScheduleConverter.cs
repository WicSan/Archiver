using ArchivePlanner.Planning.Model;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchivePlanner.Planning.Database
{
    public class BackupScheduleConverter : JsonConverter<BackupSchedule>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeof(BackupSchedule).Equals(typeToConvert);

        public override BackupSchedule Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Utf8JsonReader readerClone = reader;

            if (readerClone.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Json is no object");
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Property Type not found");
            }

            string? propertyName = readerClone.GetString();
            if (propertyName != "Type")
            {
                throw new JsonException("Property Type not found");
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Type property is not a string");
            }

            var type = Type.GetType(readerClone.GetString()!);
            return (BackupSchedule)JsonSerializer.Deserialize(ref reader, type!, options)!;
        }

        public override void Write(
            Utf8JsonWriter writer, BackupSchedule schedule, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("Type", schedule.GetType().ToString());
            writer.WriteRawValue(JsonSerializer.Serialize(schedule, schedule.GetType(), options).Replace("{", "").Replace("}", ""), true);

            writer.WriteEndObject();
        }
    }
}
