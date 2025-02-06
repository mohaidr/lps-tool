using Dashboard.Common;
using LPS.UI.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LPS.UI.Core.Host
{
    internal static class Dashboard
    {
        public static void Start(int port, string queryParams)
        {
           OpenBrowser($"http://127.0.0.1:{port}?{queryParams}");
        }
        private static void OpenBrowser(string url)
        {
            try
            {
                // Use platform-specific code to open the default web browser
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw new PlatformNotSupportedException("Unsupported platform.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening web browser: {ex.Message}");
            }
        }
    }
}
