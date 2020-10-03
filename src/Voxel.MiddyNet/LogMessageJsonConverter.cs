using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Voxel.MiddyNet
{
    internal class LogMessageJsonConverter : JsonConverter<LogMessage>
    {
        public override LogMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, LogMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Write properties.
            var propertyInfos = value.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                // Skip the Y property.
                if (propertyInfo.Name == "Properties")
                    continue;

                writer.WritePropertyName(propertyInfo.Name);
                var propertyValue = propertyInfo.GetValue(value);
                JsonSerializer.Serialize(writer, propertyValue, options);
            }

            // Write dictionary key-value pairs.
            if (value.Properties != null)
            {
                foreach (var kvp in value.Properties)
                {
                    writer.WritePropertyName(kvp.Key);
                    JsonSerializer.Serialize(writer, kvp.Value, options);
                }
            }
            
            writer.WriteEndObject();
        }
    }
}
