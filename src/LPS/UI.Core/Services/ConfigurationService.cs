using System;
using System.IO;
using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Common;

namespace LPS.UI.Core.Services
{
    public static class ConfigurationService
    {
        public static T? FetchConfiguration<T>(string configFile, IPlaceholderResolverService placeholderResolverService)
        {
            if (string.IsNullOrWhiteSpace(configFile))
            {
                throw new ArgumentException("Configuration file path cannot be null or empty.", nameof(configFile));
            }

            try
            {
                if (configFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    return SerializationHelper.Deserialize<T>(File.ReadAllText(configFile));
                }
                else if (configFile.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || configFile.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                {
                    return SerializationHelper.DeserializeFromYaml<T>(File.ReadAllText(configFile));
                }
                else
                {
                    throw new InvalidOperationException("Unsupported file format. Please use a .json or .yaml/.yml file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching configuration: {ex.Message}");
                return default;
            }
        }

        public static void SaveConfiguration<T>(string configFile, T @object)
        {
            if (string.IsNullOrWhiteSpace(configFile))
            {
                throw new ArgumentException("Configuration file path cannot be null or empty.", nameof(configFile));
            }

            if (@object == null)
            {
                throw new ArgumentNullException(nameof(@object), "SetupCommand cannot be null.");
            }

            try
            {
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(configFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (configFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    File.WriteAllText(configFile, SerializationHelper.Serialize(@object));
                }
                else if (configFile.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || configFile.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                {

                    File.WriteAllText(configFile, SerializationHelper.SerializeToYaml(@object));
                }
                else
                {
                    throw new InvalidOperationException("Unsupported file format. Please use a .json or .yaml/.yml file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }
    }
}
