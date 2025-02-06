using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace LPS.Infrastructure.Common
{
    public static class DeepCloneExtension
    {

        public static Dictionary<TKey, TValue> DeepClone<TKey, TValue>(this Dictionary<TKey, TValue> toClone)
        {
            return toClone.ToDictionary(
                entry => entry.Key,
                entry => CloneObject(entry.Value)
            );
        }

        public static TValue CloneObject<TValue>(this TValue obj)
        {
            if (obj == null)
            {
                return obj; // Null objects are directly returned
            }

            if (obj is ICloneable cloneable)
            {
                // If the object implements ICloneable, use its Clone method
                return (TValue)cloneable.Clone();
            }
            else if (obj is ValueType || obj.GetType().IsPrimitive || obj.GetType().IsEnum)
            {
                // If it's a value type or primitive type, just return the object
                return obj;
            }
            else if (obj is IEnumerable<object> enumerable)
            {
                // If it's an IEnumerable, clone each item in the collection
                Type itemType = obj.GetType().GetGenericArguments().First();
                var clonedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

                foreach (var item in enumerable)
                {
                    clonedList.Add(CloneObject(item));
                }

                return (TValue)clonedList;
            }
            else if (SerializationHelper.IsSerializable<TValue>())
            {
                // For other reference types without ICloneable, use System.Text.Json for deep copy if serializable
                return DeepCloneByJson(obj);
            }
            else
            {
                // Handle cases where the object is not serializable or doesn't implement ICloneable
                throw new InvalidOperationException($"Type {obj.GetType().FullName} is not serializable or doesn't implement ICloneable.");
            }
        }

        private static TValue DeepCloneByJson<TValue>(TValue obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            // Serialize the object to JSON
            string jsonString = JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false, // Writing indentation isn't necessary for cloning
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            });

            // Deserialize the JSON back to a new instance of the object
            return JsonSerializer.Deserialize<TValue>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

    }
}
