using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Common.LPSSerializer
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Reflection;

    public class JsonAliasConverter<T> : JsonConverter<T> where T : new()
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var instance = new T();
            var jsonDocument = JsonDocument.ParseValue(ref reader);
            var jsonElement = jsonDocument.RootElement;

            foreach (var property in typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var aliases = property.GetCustomAttributes(typeof(JsonAliasAttribute), true)
                                      .Cast<JsonAliasAttribute>()
                                      .Select(attr => attr.Alias)
                                      .Append(property.Name) // Include the property name as an alias
                                      .Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (var alias in aliases)
                {
                    if (jsonElement.TryGetProperty(alias, out var jsonProperty))
                    {
                        var propertyValue = JsonSerializer.Deserialize(jsonProperty.GetRawText(), property.PropertyType, options);
                        property.SetValue(instance, propertyValue);
                        break; // Stop looking for other aliases once a match is found
                    }
                }
            }

            return instance;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyValue = property.GetValue(value);

                if (propertyValue != null)
                {
                    var alias = property.GetCustomAttributes(typeof(JsonAliasAttribute), true)
                                        .Cast<JsonAliasAttribute>()
                                        .Select(attr => attr.Alias)
                                        .FirstOrDefault() ?? property.Name;

                    writer.WritePropertyName(alias);
                    JsonSerializer.Serialize(writer, propertyValue, options);
                }
            }

            writer.WriteEndObject();
        }
    }

}
