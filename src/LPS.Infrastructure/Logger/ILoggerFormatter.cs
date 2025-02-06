using LPS.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Logger
{
    public interface ILogFormatter
    {
        string FormatLogMessage(string dateTime, string eventId, string message, LPSLoggingLevel level);
    }
}
