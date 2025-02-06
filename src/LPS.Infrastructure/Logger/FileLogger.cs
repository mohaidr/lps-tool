using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Logging;
using System.Diagnostics;
using Spectre.Console;

namespace LPS.Infrastructure.Logger
{
    public class FileLogger : IFileLogger
    {
        private readonly TextWriter _synchronizedTextWriter;
        private readonly LoggingConfiguration _config;
        private readonly IConsoleLogger _consoleLogger;
        private readonly ILogFormatter _logFormatter;

        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public FileLogger(LoggingConfiguration config, IConsoleLogger consoleLogger, ILogFormatter logFormatter)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _consoleLogger = consoleLogger ?? throw new ArgumentNullException(nameof(consoleLogger));
            _logFormatter = logFormatter ?? throw new ArgumentNullException(nameof(logFormatter));

            SetLogFilePath(config.LogFilePath);

            string directory = Path.GetDirectoryName(_config.LogFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string fileName = Path.GetFileName(_config.LogFilePath);
            _synchronizedTextWriter = ObjectFactory.Instance.MakeSynchronizedTextWriter(Path.Combine(directory, fileName));
        }

        private void SetLogFilePath(string logFilePath)
        {
            _config.LogFilePath = string.IsNullOrWhiteSpace(logFilePath)
                ? Path.Combine("logs", "lps-logs.log")
                : logFilePath;
        }
        private int _loggingCancellationCount;
        private string _cancellationErrorMessage;
        public async Task LogAsync(string eventId, string diagnosticMessage, LPSLoggingLevel level, CancellationToken token = default)
        {
            diagnosticMessage = Markup.Escape(diagnosticMessage);
            string currentDateTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff +3:00");
            bool iSemaphoreAcquired = false;
            try
            {
                await _semaphoreSlim.WaitAsync(token);
                iSemaphoreAcquired = true;
                // Log to Console
                await _consoleLogger.LogToConsoleAsync(currentDateTime, eventId, diagnosticMessage, level, _config);
                // Log to File
                if (!_config.DisableFileLogging && level >= _config.LoggingLevel)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(_logFormatter.FormatLogMessage(currentDateTime, eventId, diagnosticMessage, level));
                    await _synchronizedTextWriter.WriteLineAsync(stringBuilder, token);
                    await _synchronizedTextWriter.FlushAsync(token);
                }
            }
            catch (OperationCanceledException ex) when (token.IsCancellationRequested)
            {
                _loggingCancellationCount++;
                _cancellationErrorMessage = $"{ex.Message} {ex.InnerException?.Message}";
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[Yellow]Warning: Logging Failed  {ex.Message} {ex.InnerException?.Message}[/]");
            }
            finally
            {
                if (iSemaphoreAcquired)
                {
                    _semaphoreSlim.Release();
                }
            }
        }

        public async Task FlushAsync()
        {
            if (_loggingCancellationCount > 0)
            {
                AnsiConsole.MarkupLine($"[Yellow]The error '{_cancellationErrorMessage.Trim()}' has been reported {_loggingCancellationCount} times[/]");
            }
            await _synchronizedTextWriter.FlushAsync();
        }

        public void Log(string eventId, string diagnosticMessage, LPSLoggingLevel level, CancellationToken token = default)
        {
           LogAsync(eventId, diagnosticMessage, level, token).Wait();
        }

        public void Flush()
        {
            FlushAsync().Wait();
        }

    }
}
