using LPS.Domain;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace LPS.UI.Core.Build.Services
{
    internal class InputPayloadService
    {

        private static string ReadFromFile(string path)
        {
            bool loop = true;
            string payload = string.Empty;
            while (loop)
            {
                try
                {
                    payload = File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Unable To Read Data From The Specified Path[/]\n{ex.Message}");
                    loop = AnsiConsole.Confirm("Would you like to retry to read the payload? (Y) to retry, (N) to continue without payload");
                    continue;
                }

                break;
            }
            return payload;
        }

        private static string ReadFromURL(string url)
        {
            bool loop = true;
            string payload = string.Empty;
            while (loop)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        payload = client.GetStringAsync(url).Result;
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Unable To Read Data From The Specified URL[/]\n{ex.Message}");
                    loop = AnsiConsole.Confirm("Would you like to retry to read the payload? (Y) to retry, (N) to continue without payload");
                    continue;
                }

                break;
            }
            return payload;
        }

        public static string Parse(string input)
        {
            if (input.StartsWith("URL:", StringComparison.OrdinalIgnoreCase))
            {
                return ReadFromURL(input.Substring(4));
            }
            else if (input.StartsWith("Path:", StringComparison.OrdinalIgnoreCase))
            {
                return ReadFromFile(input.Substring(5));
            }
            else
            {
                return input;
            }
        }

        public static string Challenge()
        {
            string input = AnsiConsole.Ask<string>("What is your [green]request payload[/]?");  
            return Parse(input);
        }
    }
}
