using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Archiver.Planning.Database
{
    public class JsonDatabase : IDisposable
    {
        private readonly string _fileName;
        private Stream? _stream;
        private JsonSerializerOptions _jsonSerializerOptions;

        public JsonDatabase(IOptions<JsonDatabaseOptions> options, IEnumerable<JsonConverter> converters)
        {
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            };

            foreach (var converter in converters)
            {
                _jsonSerializerOptions.Converters.Add(converter);
            }

            _fileName = $"{options.Value.FileName}.jdb";
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
            var idProperty = typeof(T)
                .GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType.Equals(typeof(IdAttribute))))
                .Single();

            var entities = FindAll<T>().ToDictionary(e => idProperty.GetValue(e)!);

            var id = idProperty.GetValue(entity)!;
            entities[id] = entity;

            EnsureFileStreamInitialized();

            using (var writer = new StreamWriter(_stream!))
            {
                var serializedEntities = JsonSerializer.Serialize(entities.Values.ToList(), _jsonSerializerOptions);
                writer.Write(serializedEntities);
                _stream!.SetLength(serializedEntities.Length);
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
