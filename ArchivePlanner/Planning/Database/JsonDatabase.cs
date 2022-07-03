using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchivePlanner.Planning.Database
{
    public class JsonDatabase : IDisposable
    {
        private readonly string _fileName;
        private Stream? _stream;
        private JsonSerializerOptions _jsonSerializerOptions;

        public JsonDatabase(string fileName)
        {
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                Converters =
                {
                    new LocalTimeConverter(),
                    new LocalDateTimeConverter(),
                    new FileSystemInfoConverter(),
                    new BackupScheduleConverter(),
                }
            };
            _fileName = fileName;
        }

        private void EnsureFileStreamInitialized()
        {
            if (_stream == null || !_stream.CanRead)
            {
                _stream = File.Open(_fileName, FileMode.OpenOrCreate);
            }
        }

        public IEnumerable<T> FindAll<T>()
        {
            EnsureFileStreamInitialized();

            using (var reader = new StreamReader(_stream!))
            {
                var json = reader.ReadToEnd();

                if (string.IsNullOrEmpty(json))
                    return Enumerable.Empty<T>();

                var savedObjects = JsonSerializer.Deserialize<T[]>(json, _jsonSerializerOptions);
                if (savedObjects != null)
                    return savedObjects;
                else
                    return Enumerable.Empty<T>();
            }
        }

        public void Upsert<T>(T entity)
        {
            var entities = FindAll<T>();

            EnsureFileStreamInitialized();

            using (var writer = new StreamWriter(_stream!))
            {
                entities = entities.Append(entity);
                var serializedEntities = JsonSerializer.Serialize(entities, _jsonSerializerOptions);
                writer.Write(serializedEntities);
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
