using Archiver.Shared;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Archiver.Planning.Database
{
    public class FileSystemInfoConverter : JsonConverter<FileSystemInfo>
    {
        public override FileSystemInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = reader.GetString();
            if (propertyName != "Type")
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            var type = Type.GetType(reader.GetString()!);

            reader.Read();
            reader.Read();
            var fullName = reader.GetString();
            reader.Read();

            return (FileSystemInfo?)Activator.CreateInstance(type!, fullName);
        }

        public override void Write(Utf8JsonWriter writer, FileSystemInfo value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("Type", value.GetType().ToString());
            writer.WriteString("FullName", value.FullName);

            writer.WriteEndObject();
        }
    }
}
