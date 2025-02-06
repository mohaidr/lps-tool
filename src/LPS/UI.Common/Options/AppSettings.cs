using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.UI.Common.Options
{
    public class AppSettings
    {
        public HttpClientOptions LPSHttpClientConfiguration { get; set; }
        public FileLoggerOptions LPSFileLoggerConfiguration { get; set;}
        public WatchdogOptions LPSWatchdogConfiguration { get; set; }
    }
}
