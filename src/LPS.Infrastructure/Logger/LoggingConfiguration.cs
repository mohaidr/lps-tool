using LPS.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Logger
{
    public class LoggingConfiguration
    {
        public string LogFilePath { get; set; } = "logs/lps-logs.log";
        public LPSLoggingLevel LoggingLevel { get; set; } = LPSLoggingLevel.Verbose;
        public LPSLoggingLevel ConsoleLoggingLevel { get; set; } = LPSLoggingLevel.Information;
        public bool EnableConsoleLogging { get; set; } = true;
        public bool DisableConsoleErrorLogging { get; set; } = false;
        public bool DisableFileLogging { get; set; } = false;
    }

}
