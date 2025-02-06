using CsvHelper;
using CsvHelper.Configuration;
using LPS.Domain.Common;
using LPS.Infrastructure.LPSClients.PlaceHolderService;
using System;
using System.Collections.Generic;
using LPS.Domain.Common.Interfaces;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using Newtonsoft.Json.Linq;

namespace LPS.Infrastructure.LPSClients.SessionManager
{
    public class VariableHolder : IVariableHolder
    {
        public MimeType Format { get; private set; }
        public string Pattern { get; private set; }
        public string Value { get; private set; }
        public bool IsGlobal { get; private set; }
        IPlaceholderResolverService _placeholderResolverService;
        private VariableHolder(IPlaceholderResolverService placeholderResolverService) 
        { 
            _placeholderResolverService = placeholderResolverService;
        } // Private constructor for controlled instantiation via builder

        public async Task<string> ExtractJsonValue(string jsonPath, string sessionId, CancellationToken CancellationToken)
        {
            jsonPath = (await _placeholderResolverService.ResolvePlaceholdersAsync<string>(jsonPath, sessionId, CancellationToken));
            if (Format != MimeType.ApplicationJson)
                throw new InvalidOperationException("Response is not JSON.");

            try
            {
                var json = Newtonsoft.Json.Linq.JToken.Parse(Value); // Use JToken to handle both arrays and objects
                var token = json.SelectToken(jsonPath)?.ToString()
                             ?? throw new InvalidOperationException($"JSON path '{jsonPath}' not found.");
                return ExtractRegexMatch(token, Pattern);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract JSON value using path '{jsonPath}'.", ex);
            }
        }


        public async Task<string> ExtractXmlValue(string xpath, string sessionId, CancellationToken token)
        {
            xpath = (await _placeholderResolverService.ResolvePlaceholdersAsync<string>(xpath, sessionId, token));

            if (Format != MimeType.TextXml && Format != MimeType.ApplicationXml && Format != MimeType.RawXml)
            {
                throw new InvalidOperationException("Response is not XML.");
            }

            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(Value);
                var node = doc.SelectSingleNode(xpath);
                return ExtractRegexMatch(node?.InnerText, Pattern) ?? throw new InvalidOperationException($"XPath '{xpath}' not found.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract XML value using XPath '{xpath}'.", ex);
            }
        }

        public string ExtractValueWithRegex()
        {
            return ExtractRegexMatch(Value, Pattern);
        }

        private static string ExtractRegexMatch(string value, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return value;

            try
            {
                var match = System.Text.RegularExpressions.Regex.Match(value, pattern);
                return match.Success ? match.Value : throw new InvalidOperationException($"Pattern '{pattern}' did not match.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to apply regex pattern '{pattern}'.", ex);
            }
        }

        public async Task<string> ExtractCsvValueAsync(string indices, string sessionId, CancellationToken token)
        {
            var trimmed = (await _placeholderResolverService.ResolvePlaceholdersAsync<string>(indices, sessionId, token)).Trim('[', ']');
            var parts = trimmed.Split(',');
            if (parts.Length == 2)
            {
                parts[0] = parts[0];
                parts[1] = parts[1];
            }
            if (parts.Length != 2 || !int.TryParse(parts[0], out int rowIndex) || !int.TryParse(parts[1], out int columnIndex))
            {
                throw new ArgumentException("Invalid index format. Use the format [rowIndex,columnIndex].");
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true // Ensure headers are read correctly
            };

            using (var reader = new StringReader(Value)) // Use StringReader to read from string
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<dynamic>().ToList(); // Convert records to a list for indexing

                // Adjust for skipping the header row
                if (rowIndex < 0 || rowIndex >= records.Count)
                {
                    throw new IndexOutOfRangeException("Row index is out of range.");
                }

                // Get the record (row) at the specified index
                var record = records[rowIndex];

                // Convert the record to a dictionary for column access
                var recordDict = (IDictionary<string, object>)record;

                // Ensure the column index is valid
                if (columnIndex < 0 || columnIndex >= recordDict.Count)
                {
                    throw new IndexOutOfRangeException("Column index is out of range.");
                }

                // Access the column value by index
                var value = recordDict.Values.ElementAt(columnIndex);
                return value?.ToString() ?? string.Empty;

            }
        }

        // Builder class
        public class Builder
        {
            private readonly VariableHolder _variableHolder;
            private readonly IPlaceholderResolverService _placeholderResolverService;

            public Builder(IPlaceholderResolverService placeholderResolverService)
            {
                _placeholderResolverService = placeholderResolverService ?? throw new ArgumentNullException(nameof(placeholderResolverService));
                _variableHolder = new VariableHolder(placeholderResolverService);
            }

            public Builder WithFormat(MimeType format)
            {
                _variableHolder.Format = format;
                return this;
            }

            public Builder WithPattern(string pattern)
            {
                _variableHolder.Pattern = pattern;
                return this;
            }

            public Builder WithRawValue(string value)
            {
                _variableHolder.Value = value;
                return this;
            }

            public Builder SetGlobal(bool isGlobal)
            {
                _variableHolder.IsGlobal = isGlobal;
                return this;
            }

            public async Task<VariableHolder> BuildAsync(CancellationToken token)
            {
                // Resolve placeholder if the value is a supported method
                if (!string.IsNullOrEmpty(_variableHolder.Value) && (IPlaceholderResolverService.IsSupportedPlaceHolderMethod(_variableHolder.Value) || _variableHolder.Value.StartsWith("$")))
                {
                    _variableHolder.Value = await _placeholderResolverService.ResolvePlaceholdersAsync<string>(
                        _variableHolder.Value, sessionId: null, token); // Resolve placeholder value
                }

                return _variableHolder;
            }
        }
    }
}
