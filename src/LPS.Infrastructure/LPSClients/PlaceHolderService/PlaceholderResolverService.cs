using LPS.Domain.Common;
using LPS.Infrastructure.LPSClients.SessionManager;
using LPS.Infrastructure.Caching;
using LPS.Infrastructure.LPSClients.GlobalVariableManager;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LPS.Domain.Common.Interfaces;
using System.IO;
using LPS.Infrastructure.Common;
using LPS.Infrastructure.Logger;
using LPS.Infrastructure.LPSClients.CachService;

namespace LPS.Infrastructure.LPSClients.PlaceHolderService
{
    public partial class PlaceholderResolverService : IPlaceholderResolverService
    {
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        private readonly ILogger _logger;
        private readonly PlaceholderProcessor _processor;
        public PlaceholderResolverService(
            ISessionManager sessionManager,
            ICacheService<string> memoryCacheService,
            IVariableManager variableManager,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            ILogger logger)
        {
            _runtimeOperationIdProvider = runtimeOperationIdProvider ?? throw new ArgumentNullException(nameof(runtimeOperationIdProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var paramExtractionService = new ParameterExtractorService(
                this,
                _runtimeOperationIdProvider,
                _logger);
            _processor = new PlaceholderProcessor(
                paramExtractionService,
                sessionManager,
                memoryCacheService,
                variableManager,
                runtimeOperationIdProvider,
                logger);
        }

        public async Task<T> ResolvePlaceholdersAsync<T>(string input, string sessionId, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(input))
                return default; // Return default value of the specified type

            string resolvedValue = await ParseAsync(input, sessionId, token);

            try
            {
                // Handle nullable types
                var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                if (targetType.IsEnum)
                {
                    // Attempt to parse the resolved value into the enum
                    if (Enum.TryParse(targetType, resolvedValue, true, out var enumValue))
                    {
                        return (T)enumValue;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Failed to convert placeholder value '{resolvedValue}' to enum type {targetType}.");
                    }
                }

                // For non-enum types, handle nullable and regular types
                var convertedValue = string.IsNullOrEmpty(resolvedValue)
                    ? default
                    : Convert.ChangeType(resolvedValue, targetType);

                return (T)convertedValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert placeholder value to type {typeof(T)}.", ex);
            }
        }

        private async Task<string> ParseAsync(string input, string sessionId, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(input) || !input.Contains('$'))
                return input;

            StringBuilder result = new(input.Trim());
            int currentIndex = 0;

            while (currentIndex < result.Length)
            {
                if (currentIndex + 1 < result.Length && result[currentIndex] == '$' && result[currentIndex + 1] == '$')
                {
                    result.Remove(currentIndex, 1);
                    currentIndex++;
                    continue;
                }

                if (currentIndex + 1 < result.Length && result[currentIndex] == '$')
                {
                    if (currentIndex + 2 < result.Length && result[currentIndex + 1] == '{')
                    {
                        // Handle ${variable} syntax
                        int startIndex = currentIndex + 2;
                        int stopperIndex = FindStopperIndex(result, startIndex); // the stopper will always be }

                        string placeholder = result.ToString(startIndex, stopperIndex - startIndex); // Exclude closing '}'
                        string resolvedValue = await _processor.ResolvePlaceholderAsync(placeholder, sessionId, token);

                        result.Remove(currentIndex, stopperIndex - currentIndex + 1); // to remove }
                        result.Insert(currentIndex, resolvedValue);
                        currentIndex += resolvedValue.Length;
                    }
                    else
                    {
                        int startIndex = currentIndex + 1;
                        int stopperIndex = FindStopperIndex(result, startIndex); // Stoppers like / ; , ] } etc., indicate the end of a variable. For example, in $x,$y, the ',' acts as a stopper, signaling that $x is a complete placeholder to resolve, so $x,$y should be treated as two separate variables.
                        string placeholder = result.ToString(startIndex, stopperIndex - startIndex);
                        string resolvedValue = await _processor.ResolvePlaceholderAsync(placeholder, sessionId, token);

                        result.Remove(currentIndex, stopperIndex - currentIndex);
                        result.Insert(currentIndex, resolvedValue);
                        currentIndex += resolvedValue.Length;
                    }
                }
                else
                {
                    currentIndex++;
                }
            }

            return result.ToString();
        }

        private static int FindStopperIndex(StringBuilder result, int startIndex)
        {
            int endIndex = startIndex;
            bool insideParentheses;
            bool insideSquareBracket;
            int parenthesesBalance = 0;
            int squareBracketBalance = 0;

            // Check for ${variable} syntax
            if (startIndex > 1 && result[startIndex - 2] == '$' && result[startIndex - 1] == '{')
            {
                // Look for the matching closing '}'
                while (endIndex < result.Length)
                {
                    if (result[endIndex] == '}')
                    {
                        return endIndex; 
                    }
                    endIndex++;
                }

                throw new InvalidOperationException("Unmatched '{' in variable.");
            }


            char[] pathChars = ['.', '/', '[', ']'];
            bool isMethod = false;
            char lastChar = ' ';
            char currentChar = ' ';
            while (endIndex < result.Length)
            {
                currentChar = result[endIndex];
                if (currentChar == '(') { parenthesesBalance++; isMethod = true; }
                if (currentChar == ')') parenthesesBalance--;
                if (currentChar == '[') { squareBracketBalance++;};
                if (currentChar == ']') { squareBracketBalance--; };
                insideParentheses = parenthesesBalance > 0;
                insideSquareBracket = squareBracketBalance > 0;
                if ((!insideParentheses && !insideSquareBracket &&
                    !char.IsLetterOrDigit(currentChar) && !pathChars.Contains(currentChar)) 
                    || parenthesesBalance<0 
                    || squareBracketBalance <0)
                {
                    break;
                }
                lastChar = currentChar;
                endIndex++;
            }

            /*
             For methods, the endIndex is increased to ensure the closing ')' is not excluded from the placeholder name.
             For variables, the stopper should not be part of the variable name. The caller method determines the placeholder name by subtracting the start index (the first letter after $) from the stopper index.
             For example, in $x_$y, the stopper index is 2, and the start index is 1. Subtracting 1 from 2 gives the variable name length (1), allowing us to extract the variable name by slicing from the start index for the calculated length.
            */
            if (isMethod && currentChar ==')' && parenthesesBalance == 0)
            {
                endIndex++;
            }
            // handle the case "[$x, $y]" so we do not take ] with $y
            if (squareBracketBalance !=0 
                && (lastChar == '[' || lastChar == ']')) 
                endIndex--;

            return endIndex;
        }

    }

    internal class PlaceholderProcessor
    {
        private readonly ISessionManager _sessionManager;
        private readonly IVariableManager _variableManager;
        private readonly ICacheService<string> _memoryCacheService;
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        private readonly ILogger _logger;
        private readonly ParameterExtractorService _paramService;
        public PlaceholderProcessor(
            ParameterExtractorService paramService,
            ISessionManager sessionManager,
            ICacheService<string> memoryCacheService,
            IVariableManager variableManager,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            ILogger logger)
        {
            _sessionManager = sessionManager;
            _memoryCacheService = memoryCacheService;
            _variableManager = variableManager;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _logger = logger;
            _paramService = paramService;
        }


        public async Task<string> ResolvePlaceholderAsync(string placeholder, string sessionId, CancellationToken token)
        {
            if (IPlaceholderResolverService.IsSupportedPlaceHolderMethod($"${placeholder}"))
            {
                return await ResolveMethodAsync(placeholder, sessionId, token);
            }
            else
            {
                return await ResolveVariableAsync(placeholder, sessionId, token);
            }
        }

        private async Task<string> ResolveMethodAsync(string placeholder, string sessionId, CancellationToken token)
        {
            int openParenIndex = placeholder.IndexOf('(');
            string functionName = placeholder.Substring(0, openParenIndex).Trim();
            string parameters = placeholder.Substring(openParenIndex + 1, placeholder.Length - openParenIndex - 2).Trim();
            return functionName.ToLowerInvariant() switch
            {
                "random" => await GenerateRandomStringAsync(parameters, sessionId, token),
                "randomnumber" => await GenerateRandomNumberAsync(parameters, sessionId, token),
                "timestamp" => await GenerateTimestampAsync(parameters, sessionId, token),
                "guid" => Guid.NewGuid().ToString(),
                "base64encode" => await Base64EncodeAsync(parameters, sessionId, token),
                "hash" => await GenerateHashAsync(parameters, sessionId, token),
                "read" => await ReadFileAsync(parameters, sessionId, token),
                "loopcounter" => await LoopCounterAsync(parameters, sessionId, token),
                _ => throw new InvalidOperationException($"Unknown function: {functionName}")
            };
        }

        private async Task<string> ResolveVariableAsync(string placeholder, string sessionId, CancellationToken token)
        {
            string cacheKey = $"{CachePrefixes.Placeholder}{placeholder}";
            if (_memoryCacheService.TryGetItem(cacheKey, out string cachedResult))
            {
                return cachedResult;
            }

            string variableName = placeholder;
            string path = null;

            if (placeholder.Contains('.') || placeholder.Contains('/') || placeholder.Contains('['))
            {
                int splitIndex = placeholder.IndexOfAny(new[] { '.', '/', '[' });
                variableName = placeholder.Substring(0, splitIndex);
                path = placeholder.Substring(splitIndex);
            }

            var variableHolder = await _sessionManager.GetResponseAsync(sessionId, variableName, token)
                              ?? await _variableManager.GetVariableAsync(variableName, token);

            if (variableHolder == null)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Variable '{variableName}' not found.", LPSLoggingLevel.Warning, token);
                return $"${variableName}";
            }

            string resolvedValue = !string.IsNullOrEmpty(path)
                ? await ExtractValueFromPathAsync(variableHolder, path, sessionId, token)
                : variableHolder.ExtractValueWithRegex();

            if(path== null || (!path.Contains('$') && !string.IsNullOrWhiteSpace(sessionId))) // No cache to handle a case where a method or variable is embedded in a path so it has to be resolved with every request, e.g ${csvData[$loopcounter(start=0, end=5, counter=test),0]}
                await _memoryCacheService.SetItemAsync(cacheKey, resolvedValue, !string.IsNullOrEmpty(sessionId) ? TimeSpan.FromSeconds(30): TimeSpan.MaxValue);
            
            return resolvedValue;
        }

        private static async Task<string> ExtractValueFromPathAsync(IVariableHolder variableHolder, string path, string sessionId, CancellationToken token)
        {
            if (path.StartsWith(".") || path.StartsWith("[") && variableHolder.Format == MimeType.ApplicationJson)
            {
                return await variableHolder.ExtractJsonValue(path, sessionId, token);
            }
            else if (path.StartsWith("/") &&
                     (variableHolder.Format == MimeType.ApplicationXml || variableHolder.Format == MimeType.TextXml || variableHolder.Format == MimeType.RawXml))
            {
                return await variableHolder.ExtractXmlValue(path, sessionId, token);
            }
            else if (path.StartsWith("[") && variableHolder.Format == MimeType.TextCsv)
            {
                return await variableHolder.ExtractCsvValueAsync(path, sessionId, token);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported path '{path}' for a variable of type '{variableHolder.Format}'.");
            }
        }

        private async Task<string> GenerateRandomStringAsync(string parameters, string sessionId, CancellationToken token)
        {
            int length = await _paramService.ExtractNumberAsync(parameters, "length", 10, sessionId, token);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<string> GenerateRandomNumberAsync(string parameters, string sessionId, CancellationToken token)
        {
            int min = await _paramService.ExtractNumberAsync(parameters, "min", 1, sessionId, token);
            int max = await _paramService.ExtractNumberAsync(parameters, "max", 100, sessionId, token);
            var random = new Random();
            return random.Next(min, max + 1).ToString();
        }

        private async Task<string> GenerateTimestampAsync(string parameters, string sessionId, CancellationToken token)
        {
            string format = await _paramService.ExtractStringAsync(parameters, "format", "yyyy-MM-ddTHH:mm:ss", sessionId, token);
            return DateTime.UtcNow.ToString(format);
        }

        private async Task<string> Base64EncodeAsync(string parameters, string sessionId, CancellationToken token)
        {
            string value = await _paramService.ExtractStringAsync(parameters, "value", string.Empty, sessionId, token);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
        private async Task<string> GenerateHashAsync(string parameters, string sessionId, CancellationToken token)
        {
            string value = await _paramService.ExtractStringAsync(parameters, "value", string.Empty, sessionId, token);
            string algorithm = await _paramService.ExtractStringAsync(parameters, "algorithm", "SHA256", sessionId, token);

            using var hasher = algorithm switch
            {
                "MD5" => System.Security.Cryptography.MD5.Create(),
                "SHA256" => System.Security.Cryptography.SHA256.Create(),
                "SHA1" => (System.Security.Cryptography.HashAlgorithm)System.Security.Cryptography.SHA1.Create(),
                _ => throw new InvalidOperationException($"Unsupported hash algorithm: {algorithm}")
            };

            byte[] hash = hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private async Task<string> ReadFileAsync(string parameters, string sessionId, CancellationToken token)
        {
            string filePath = await _paramService.ExtractStringAsync(parameters, "path", string.Empty, sessionId, token);

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(parameters));

            string fullPath = Path.GetFullPath(filePath, AppConstants.EnvironmentCurrentDirectory);
            // Check if the file content is already cached
            string pathCacheKey = $"{CachePrefixes.Path}{fullPath}";
            if (_memoryCacheService.TryGetItem(pathCacheKey, out string cachedContent))
            {
                return cachedContent;
            }

            if (!File.Exists(fullPath))
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"File '{fullPath}' does not exist.", LPSLoggingLevel.Warning, token);
                return string.Empty;
            }

            try
            {
                using var reader = new StreamReader(fullPath, Encoding.UTF8);
                string fileContent = await reader.ReadToEndAsync();

                // Cache the file content for the program's lifetime
                await _memoryCacheService.SetItemAsync(pathCacheKey, fileContent, TimeSpan.MaxValue);

                return fileContent;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Error reading file '{fullPath}': {ex.Message}", LPSLoggingLevel.Error, token);
                throw;
            }
        }

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<string> LoopCounterAsync(string parameters, string sessionId, CancellationToken token)
        {
            // Extract parameters for start and end values
            var startValue = await _paramService.ExtractNumberAsync(parameters, "start", 0, sessionId, token);
            var endValue = await _paramService.ExtractNumberAsync(parameters, "end", 100000, sessionId, token);
            var counterName =  await _paramService.ExtractStringAsync(parameters, "counter", string.Empty, sessionId, token);
            var counterNameCachePart = !string.IsNullOrEmpty(counterName) ? $"_{counterName.Trim()}" : string.Empty;
            if (startValue >= endValue)
            {
                throw new ArgumentException("startValue must be less than endValue.");
            }

            // Determine cache key
            string cacheKey = string.IsNullOrEmpty(sessionId) || !int.TryParse(sessionId, out _)
                ? $"{CachePrefixes.GlobalCounter}{startValue}_{endValue}{counterNameCachePart}"
                : $"{CachePrefixes.SessionCounter}{sessionId}_{startValue}_{endValue}{counterNameCachePart}";

            await _semaphore.WaitAsync(token); // Lock to ensure thread-safety
            try
            {
                // Retrieve the current value from the cache or initialize to startValue
                if (!_memoryCacheService.TryGetItem(cacheKey, out string currentValueString) || !int.TryParse(currentValueString, out int currentValue))
                {
                    currentValue = startValue;
                }
                else
                {
                    currentValue++;
                    if (currentValue > endValue || currentValue < startValue)
                    {
                        currentValue = startValue; // Restart counter
                        await _logger.LogAsync(
                            _runtimeOperationIdProvider.OperationId,
                            $"Cache key '{cacheKey}': Counter reset to start value '{startValue}' because current value '{currentValue}' exceeded end value '{endValue}' or fell below start value.",
                            LPSLoggingLevel.Information,
                            token
                        );
                    }
                }

                // Update the cache with the new value
                await _memoryCacheService.SetItemAsync(cacheKey, currentValue.ToString(), TimeSpan.MaxValue);

                return currentValue.ToString();
            }
            finally
            {
                _semaphore.Release(); // Release the lock
            }
        }
    }

    public class ParameterExtractorService
    {
        private readonly PlaceholderResolverService _resolver;
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        private readonly ILogger _logger;

        public ParameterExtractorService(
            PlaceholderResolverService resolver,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            ILogger logger)
        {
            _resolver = resolver;
            _runtimeOperationIdProvider = runtimeOperationIdProvider ?? throw new ArgumentNullException(nameof(runtimeOperationIdProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }


        public async Task<int> ExtractNumberAsync(string parameters, string key, int defaultValue, string sessionId, CancellationToken token)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                return defaultValue;
            }

            var keyValuePairs = parameters.Split(',');
            foreach (var pair in keyValuePairs)
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2 && parts[0].Trim() == key)
                {
                    int resolvedValue = await _resolver.ResolvePlaceholdersAsync<int>(parts[1].Trim(), sessionId, token);
                    return resolvedValue;
                }
            }

            return defaultValue;
        }

        public async Task<string> ExtractStringAsync(string parameters, string key, string defaultValue, string sessionId, CancellationToken token)
        {
            if (string.IsNullOrEmpty(parameters))
                return defaultValue;

            var keyValuePairs = parameters.Split(',');
            foreach (var pair in keyValuePairs)
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2 && parts[0].Trim() == key)
                {
                    return await _resolver.ResolvePlaceholdersAsync<string>(parts[1].Trim(), sessionId, token);
                }
            }

            return defaultValue;
        }
    }
}
