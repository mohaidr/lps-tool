using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.UI.Common.Options
{
    public class AppSettings
    {
        public HttpClientOptions HttpClient { get; set; }
        public FileLoggerOptions FileLogger { get; set;}
        public WatchdogOptions Watchdog { get; set; }
    }
}
