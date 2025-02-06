using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.LPSRequest.LPSHttpRequest
{
    public class URL: IValueObject
    {
        public URL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty.", nameof(url));

            Url = url;

            // Split the URL into its main parts
            var mainParts = url.Split(new[] { "://" }, 2, StringSplitOptions.None);

            if (mainParts.Length == 2)
            {
                Schema = IsPlaceholder(mainParts[0]) || IsValidSchema(mainParts[0]) ? mainParts[0] : null;
                var rest = mainParts[1];
                ParseRest(rest);
            }
            else
            {
                Schema = null;
                ParseRest(mainParts[0]);
            }
        }

        public string BaseUrl => string.Concat(Schema, $"{(!string.IsNullOrEmpty(Schema)?"://":string.Empty)}", HostName);
        public string Url { get; private set; }
        public string HostName { get; private set; }
        public string Schema { get; private set; }
        private List<KeyValuePair<string, string>> QueryParameters { get; set; } = new();
        private List<string> PathParameters { get; set; } = new();

        private void ParseRest(string rest)
        {
            var queryIndex = rest.IndexOf('?');

            // Separate path and query
            var path = queryIndex >= 0 ? rest.Substring(0, queryIndex) : rest;
            var query = queryIndex >= 0 ? rest.Substring(queryIndex + 1) : string.Empty;

            // Parse hostname and path
            var pathParts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathParts.Length > 0)
            {
                HostName = IsPlaceholder(pathParts[0]) || IsValidHostname(pathParts[0]) ? pathParts[0] : null;
                PathParameters = pathParts.Skip(1).ToList();
            }

            // Parse query parameters
            if (!string.IsNullOrEmpty(query))
            {
                QueryParameters = query.Split('&')
                                       .Select(q =>
                                       {
                                           var parts = q.Split('=', 2);
                                           return new KeyValuePair<string, string>(
                                               parts[0],
                                               parts.Length > 1 ? parts[1] : string.Empty
                                           );
                                       })
                                       .ToList();
            }
        }

        private bool IsPlaceholder(string value)
        {
            return !string.IsNullOrEmpty(value) && value.StartsWith("$");
        }

        private bool IsValidSchema(string value)
        {
            // Validate common schemas like http, https, ftp, etc.
            return new[] { "http", "https", "ftp" }.Contains(value.ToLower());
        }

        private bool IsValidHostname(string value)
        {
            // Basic validation for hostnames
            return Uri.CheckHostName(value) != UriHostNameType.Unknown;
        }

        public string GetCombinedPathParameters()
        {
            return string.Join("/", PathParameters);
        }

        public string GetCombinedQueryParameters()
        {
            return string.Join("&", QueryParameters.Select(q => $"{q.Key}={q.Value}"));
        }
    }

}
