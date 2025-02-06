using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LPS.UI.Common.Options;
using System.Text.Json;

namespace LPS.UI.Common.Extensions
{
    public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
    {
        private readonly IHostEnvironment _environment;
        private readonly IOptionsMonitor<T> _options;
        private readonly IConfigurationRoot _configuration;
        private readonly string _section;
        private readonly string _appSettingsFileLocation;

        public WritableOptions(
            IHostEnvironment environment,
            IOptionsMonitor<T> options,
            IConfigurationRoot configuration,
            string section,
            string appSettingsFileLocation)
        {
            _environment = environment;
            _options = options;
            _configuration = configuration;
            _section = section;
            _appSettingsFileLocation = appSettingsFileLocation;
        }

        public T Value => _options.CurrentValue;
        public T Get(string name) => _options.Get(name);

        public void Update(Action<T> applyChanges)
        {
            T updatedObject = new T();
            applyChanges(updatedObject);

            var jsonContent = File.ReadAllText(_appSettingsFileLocation);
            var jsonObject = JObject.Parse(jsonContent);

            // Split the property path into individual property names
            var propertyNames = _section.Split(':');

            if (propertyNames.Length > 0)
            {
                // Traverse the JSON structure based on the property path
                var currentToken = jsonObject;
                for (int i = 0; i < propertyNames.Length - 1; i++)
                {
                    if (currentToken[propertyNames[i]] == null)
                    {
                        throw new ArgumentException($"Property '{propertyNames[i]}' not found in the JSON.");
                    }

                    currentToken = (JObject)currentToken[propertyNames[i]];
                }
                // Update the target property with the updated value
                currentToken[propertyNames[^1]] = JToken.FromObject(updatedObject);

                // Serialize the JSON back to a string
                var updatedJson = jsonObject.ToString(Newtonsoft.Json.Formatting.Indented);

                // Save the updated JSON back to the file
                File.WriteAllText(_appSettingsFileLocation, updatedJson);
            }
            else
            {
                throw new ArgumentException("Invalid property path provided.");
            }
        }
       
    }
}
