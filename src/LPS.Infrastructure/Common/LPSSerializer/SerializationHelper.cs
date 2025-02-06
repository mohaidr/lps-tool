using System;
using System.Collections;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Collections.Generic;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Core;
using LPS.Infrastructure.Common.LPSSerializer;
namespace LPS.Infrastructure.Common
{
    public static class SerializationHelper
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters = { new JsonAliasConverter<object>() },
        };
        private static readonly DefaultValuesHandling YamlCurrentDefaultValuesHandling = DefaultValuesHandling.OmitDefaults;

        private static readonly ISerializer YamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(YamlCurrentDefaultValuesHandling)
            .Build();

        private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new YamlAliasConverter())
            .Build();

        public static bool IsSerializable<T>(Type type = null)
        {
            type = type ?? typeof(T);
            if (IsSerializableAttribute(type))
            {
                return true;
            }

            try
            {
                object obj = Activator.CreateInstance(type);
                JsonSerializer.Serialize(obj);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsSerializableAttribute(Type type)
        {
            return type.GetCustomAttributes(typeof(SerializableAttribute), true).Any();
        }

        public static string Serialize<T>(T obj)
        {
            try
            {
                var serializableObject = obj;
                if (JsonSerializerOptions.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault)
                {
                    CleanDefaultValues(serializableObject);
                }
                return JsonSerializer.Serialize(serializableObject, JsonSerializerOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Serialization Has Failed: {ex.Message}");
            }
        }

        public static string SerializeToYaml<T>(T obj)
        {
            try
            {
                var serializableObject = obj;
                if (YamlCurrentDefaultValuesHandling == DefaultValuesHandling.OmitDefaults)
                {
                    CleanDefaultValues(serializableObject);
                }
                return YamlSerializer.Serialize(serializableObject);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"YAML Serialization Has Failed: {ex.Message}");
            }
        }

        public static T Deserialize<T>(string jsonString)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonString, JsonSerializerOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Deserialization Has Failed: {ex.Message}");
            }
        }

        public static T DeserializeFromYaml<T>(string yamlString)
        {
            try
            {
                return YamlDeserializer.Deserialize<T>(yamlString);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"YAML Deserialization Has Failed: {ex.Message} {ex.InnerException?.Message}");
            }
        }
        private static T CleanDefaultValues<T>(T obj)
        {
            if (obj == null) return default;

            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Skip properties that have index parameters or no setters
                if (prop.GetIndexParameters().Length > 0 || !prop.CanWrite)
                {
                    continue;
                }

                var value = prop.GetValue(obj);

                // Recursively clean nested objects
                if (value != null && !IsDefaultValue(value))
                {
                    if (value is IEnumerable enumerable && !(value is string))
                    {
                        foreach (var item in enumerable)
                        {
                            CleanDefaultValues(item);
                        }
                    }
                    else
                    {
                        CleanDefaultValues(value);
                    }
                }

                // Set property to null if it has a default value
                if (IsDefaultValue(value))
                {
                    prop.SetValue(obj, null);
                }
            }

            return obj;
        }


        public static bool IsDefaultValue(object value)
        {
            if (value == null) return true;

            Type type = value.GetType();

            if (type.IsValueType && Activator.CreateInstance(type)?.Equals(value) == true)
                return true;

            if (value is IEnumerable && !((IEnumerable)value).Cast<object>().Any())
                return true;

            if (type == typeof(string) && string.IsNullOrEmpty((string)value))
                return true;

            return false;
        }
    }
}
