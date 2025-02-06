using System;
using System.Collections.Generic;
using System.Linq;
using LPS.Infrastructure.Common;
using Spectre.Console;

namespace LPS.UI.Core.Build.Services
{
    internal class InputHeaderService
    {
        public static Dictionary<string, string> Challenge()
        {
            var kvp = new List<string>();

            Dictionary<string, string> httpheaders = new Dictionary<string, string>();
            while (true)
            {
                string input = Console.ReadLine();
                if (input.Equals("C", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                try
                {
                    if (string.IsNullOrEmpty(input))
                       continue;
                    kvp = input.Split(':').ToList<string>();
                    if (!httpheaders.ContainsKey(kvp.First().Trim()))
                        httpheaders.Add(kvp.First().Trim(), string.Join(":", kvp.Where(str => str != kvp.First())).Trim());
                    else
                        httpheaders[kvp[0].Trim()] = string.Join(":", kvp.Where(str => str != kvp.First())).Trim();
                }
                catch
                {
                    AnsiConsole.MarkupLine("[red]Invalid header[/]");
                }
            }
            return httpheaders.DeepClone();
        }

        public static Dictionary<string, string> Parse(IList<string> headers)
        {
            var kvp = new List<string>();
            Dictionary<string, string> httpheaders = new Dictionary<string, string>();

            foreach (string header in headers)
            {
                try
                {
                    kvp = header.Split(':').ToList<string>();
                    if (!httpheaders.ContainsKey(kvp.First().Trim()))
                        httpheaders.Add(kvp.First().Trim(), string.Join(":", kvp.Where(str => str != kvp.First())));
                    else
                        httpheaders[kvp.First()] = string.Join(":", kvp.Where(str => str != kvp.First()));
                }
                catch
                {
                    AnsiConsole.MarkupLine($"[red]Invalid header[/] {header}");
                }
            }
            return httpheaders.DeepClone();
        }

    }
}
