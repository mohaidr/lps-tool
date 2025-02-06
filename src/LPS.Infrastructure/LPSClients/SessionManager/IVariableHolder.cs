using LPS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.SessionManager
{
    public interface IVariableHolder
    {
        public static bool IsKnownSupportedFormat(MimeType mimeType)
        {
            return mimeType == MimeType.ApplicationJson ||
                   mimeType == MimeType.RawXml ||
                   mimeType == MimeType.TextXml ||
                   mimeType == MimeType.ApplicationXml ||
                   mimeType == MimeType.TextPlain ||
                   mimeType == MimeType.TextCsv;
        }
        public string Value { get;}
        public MimeType Format { get; }
        public string Pattern { get; }
        public bool IsGlobal { get; }
        public Task<string> ExtractJsonValue(string pattern, string sessionId, CancellationToken token);
        public Task<string> ExtractXmlValue(string xpath, string sessionId, CancellationToken token);
        public Task<string> ExtractCsvValueAsync(string indices, string sessionId, CancellationToken token);
        public string ExtractValueWithRegex();
    }
}
