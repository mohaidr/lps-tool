using System;
using System.Threading.Tasks;
using System.Threading;
using Spectre.Console;
using LPS.UI.Core.Host;
using Microsoft.Extensions.DependencyInjection;
using LPS.UI.Core.LPSValidators;
using LPS.Infrastructure.Nodes;
using Grpc.Net.Client;

namespace LPS
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AnsiConsole.Write(new FigletText("Load -- Perform {} Stress ^ ").Centered().Color(Color.Green));
            //DI Services
            var host = Startup.ConfigureServices(args);
            var cancelltionToken = host.Services.GetRequiredService<CancellationTokenSource>();

           
            await host.StartAsync(cancelltionToken.Token);
            await host.StopAsync(cancelltionToken.Token);
        }

    }
}
