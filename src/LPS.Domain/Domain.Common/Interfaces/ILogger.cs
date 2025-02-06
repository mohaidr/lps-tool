using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.Common.Interfaces
{

    public enum LPSLoggingLevel
    {
        Verbose,
        Information,
        Warning,
        Error,
        Critical,
    }
    public interface ILogger
    {
        void Log(string eventId, string diagnosticMessage, LPSLoggingLevel level, CancellationToken token = default);
        Task LogAsync(string eventId, string diagnosticMessage, LPSLoggingLevel level, CancellationToken token = default);
        void Flush();
        Task FlushAsync();
    }
}
