using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace LPS.Domain.Common.Interfaces
{
    public interface IPlaceholderResolverService
    {

        /// <summary>
        /// Resolves placeholders within the given input string and converts the resolved value to the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The target type to which the resolved placeholder value should be converted.</typeparam>
        /// <param name="input">The input string containing placeholders to be resolved. If null or empty, the default value of <typeparamref name="T"/> is returned.</param>
        /// <param name="sessionId">A unique identifier for the session, used to resolve placeholders contextually.</param>
        /// <param name="token">A <see cref="CancellationToken"/> to observe while performing asynchronous operations.</param>
        /// <returns>
        /// An instance of type <typeparamref name="T"/> representing the converted value of the resolved placeholder.
        /// If the input is null, whitespace, or resolves to an empty string, the default value of <typeparamref name="T"/> is returned.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the resolved value cannot be converted to the specified type <typeparamref name="T"/>.
        /// </exception>
        /// <remarks>
        /// This method supports conversion to both nullable and non-nullable types:
        /// <list type="bullet">
        /// <item>If the target type is an enum, the method attempts to parse the resolved value into the corresponding enum value.</item>
        /// <item>For non-enum types, the method uses <see cref="Convert.ChangeType"/> to perform the conversion.</item>
        /// <item>If the conversion fails, an <see cref="InvalidOperationException"/> is thrown with detailed error information.</item>
        /// </list>
        /// If the input is null, whitespace, or resolves to an empty string, the default value of <typeparamref name="T"/> is returned.
        /// </remarks>
        Task<T> ResolvePlaceholdersAsync<T>(string input, string sessionId, CancellationToken Token);
        /// <summary>
        /// Determines whether the given string represents a supported placeholder method.
        /// </summary>
        /// <param name="value">The string to evaluate, expected to be in the format of a placeholder method (e.g., "$methodName(arguments)").</param>
        /// <returns>
        /// <c>true</c> if the input string matches a supported placeholder method; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method performs the following checks to determine validity:
        /// <list type="bullet">
        /// <item>Ensures the input string is not null, empty, or whitespace.</item>
        /// <item>Validates that the string starts with <c>'$'</c> and ends with <c>')'</c>.</item>
        /// <item>Extracts the method name by identifying the substring before the first opening parenthesis <c>'('</c>.</item>
        /// <item>Checks the method name against a predefined list of supported methods (case-insensitive).</item>
        /// </list>
        /// Supported methods include:
        /// <c>random</c>, <c>randomnumber</c>, <c>timestamp</c>, <c>guid</c>, <c>base64encode</c>, <c>hash</c>, <c>customvariable</c>, and <c>read</c>.
        /// </remarks>
        static bool IsSupportedPlaceHolderMethod(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("$") || !value.EndsWith(")"))
                return false;

            // Extract function name
            int openParenIndex = value.IndexOf('(');
            if (openParenIndex == -1)
                return false;

            string functionName = value.Substring(1, openParenIndex - 1).Trim().ToLowerInvariant(); // Skip $

            // Check against supported methods
            return functionName switch
            {
                "random" => true,
                "randomnumber" => true,
                "timestamp" => true,
                "guid" => true,
                "base64encode" => true,
                "hash" => true,
                "read" => true,
                "loopcounter" => true,
                _ => false // Unsupported method
            };
        }
    }
}

