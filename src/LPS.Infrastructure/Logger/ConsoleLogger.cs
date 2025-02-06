using LPS.Domain.Common.Interfaces;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Logger
{
    public class ConsoleLogger : IConsoleLogger
    {
        public async Task LogToConsoleAsync(string dateTime, string eventId, string message, LPSLoggingLevel level, LoggingConfiguration config)
        {
            if (level < config.ConsoleLoggingLevel || !config.EnableConsoleLogging)
                return;

            string logMessage = $"[[{level}]] {dateTime} {eventId} {message}";

            if (level == LPSLoggingLevel.Verbose || level == LPSLoggingLevel.Information)
            {
                AnsiConsole.MarkupLine($"[blue]{logMessage}[/]");
            }
            else if (level == LPSLoggingLevel.Warning && !config.DisableConsoleErrorLogging)
            {
                AnsiConsole.MarkupLine($"[yellow]{logMessage}[/]");
            }
            else if ((level == LPSLoggingLevel.Error || level == LPSLoggingLevel.Critical) && !config.DisableConsoleErrorLogging)
            {
                AnsiConsole.MarkupLine($"[red]{logMessage}[/]");
            }

            await Task.CompletedTask; // Simulating async behavior
        }
    }

}
