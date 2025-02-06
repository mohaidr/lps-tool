using LPS.Infrastructure.Common.LPSSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LPS.Infrastructure.Common
{
    public class YamlAliasConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            // Apply only to classes with alias attributes
            return type.IsClass && type.GetProperties().Any(p => p.IsDefined(typeof(YamlAliasAttribute), true));
        }
        public object ReadYaml(IParser parser, Type type, ObjectDeserializer deserializer)
        {
            var dictionary = deserializer(typeof(Dictionary<string, object>)) as Dictionary<string, object>;
            if (dictionary == null)
            {
                throw new InvalidOperationException($"Failed to deserialize YAML into a dictionary for type {type.Name}");
            }

            var instance = Activator.CreateInstance(type);

            foreach (var prop in type.GetProperties())
            {
                var aliases = prop.GetCustomAttributes(typeof(YamlAliasAttribute), true)
                                  .Cast<YamlAliasAttribute>()
                                  .Select(attr => attr.Alias)
                                  .Append(prop.Name)
                                  .Distinct(StringComparer.OrdinalIgnoreCase)
                                  .ToList();

                var match = dictionary.FirstOrDefault(kvp => aliases.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(match.Key) && match.Value != null)
                {
                    try
                    {
                        var value = match.Value;

                        // Handle Dictionary<string, string>
                        if (prop.PropertyType == typeof(Dictionary<string, string>))
                        {
                            var stringDict = ((IDictionary<object, object>)value)
                                .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString());
                            prop.SetValue(instance, stringDict);
                        }
                        // Handle nested complex objects
                        else if (value is IDictionary<object, object> dictValue)
                        {
                            var nestedYamlString = SerializeDictionaryToYaml(dictValue);
                            var nestedObject = new DeserializerBuilder()
                                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                .Build()
                                .Deserialize(nestedYamlString, prop.PropertyType);
                            prop.SetValue(instance, nestedObject);
                        }
                        // Handle collections
                        else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                        {
                            var elementType = prop.PropertyType.IsArray
                                ? prop.PropertyType.GetElementType()
                                : prop.PropertyType.GetGenericArguments().FirstOrDefault();

                            var list = ((IEnumerable<object>)value)
                                .Select(item => ConvertValue(item, elementType, deserializer))
                                .ToList();

                            if (prop.PropertyType.IsArray)
                            {
                                var array = Array.CreateInstance(elementType, list.Count);
                                Array.Copy(list.ToArray(), array, list.Count);
                                prop.SetValue(instance, array);
                            }
                            else
                            {
                                var typedList = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                                foreach (var item in list) typedList.Add(item);
                                prop.SetValue(instance, typedList);
                            }
                        }
                        // Handle primitive types
                        else if (value is IConvertible)
                        {
                            var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                            prop.SetValue(instance, convertedValue);
                        }
                        else
                        {
                            // Fallback for direct assignment
                            prop.SetValue(instance, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to set property '{prop.Name}' with value '{match.Value}': {ex.Message}", ex);
                    }
                }
            }

            return instance;
        }

        private static object ConvertValue(object value, Type targetType, ObjectDeserializer deserializer)
        {
            if (value == null)
                return null;

            if (targetType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            if (value is IDictionary<string, object> dict)
            {
                return deserializer(targetType);
            }

            if (typeof(IEnumerable).IsAssignableFrom(targetType) && targetType != typeof(string))
            {
                var elementType = targetType.IsArray
                    ? targetType.GetElementType()
                    : targetType.GetGenericArguments().FirstOrDefault();

                if (elementType != null && value is IEnumerable<object> enumerable)
                {
                    var list = enumerable.Select(item => ConvertValue(item, elementType, deserializer)).ToList();

                    if (targetType.IsArray)
                    {
                        var array = Array.CreateInstance(elementType, list.Count);
                        Array.Copy(list.ToArray(), array, list.Count);
                        return array;
                    }

                    var typedList = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                    foreach (var item in list)
                    {
                        typedList.Add(item);
                    }
                    return typedList;
                }
            }

            if (value is IConvertible)
            {
                return Convert.ChangeType(value, targetType);
            }

            throw new InvalidOperationException($"Cannot convert value '{value}' to type '{targetType.Name}'.");
        }

        private static string SerializeDictionaryToYaml(IDictionary<object, object> dictionary)
        {
            var yamlSerializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return yamlSerializer.Serialize(dictionary);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var prop in type.GetProperties())
            {
                // Use the first alias or default to the property name
                var alias = prop.GetCustomAttributes(typeof(YamlAliasAttribute), true)
                                .Cast<YamlAliasAttribute>()
                                .Select(attr => attr.Alias)
                                .FirstOrDefault() ?? prop.Name;

                var propValue = prop.GetValue(value);
                if (propValue != null)
                {
                    dictionary[alias] = propValue;
                }
            }

            serializer(dictionary);
        }
    }

}