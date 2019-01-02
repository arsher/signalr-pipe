using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SignalR.Pipes.Common;

namespace FunctionalTests
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            string url = null;
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--url":
                        i += 1;
                        url = args[i];
                        break;
                }
            }

            var uri = new Uri(url);

            var hostUrlBuilder = new UriBuilder();
            hostUrlBuilder.Scheme = uri.Scheme;
            hostUrlBuilder.Host = uri.Host; 

            Console.WriteLine($"Using host url: {hostUrlBuilder.Uri}");

            var host = new HostBuilder()
                    .UseConsoleLifetime()
                    .UseHostUri(hostUrlBuilder.Uri)
                   .ConfigureLogging((hostContext, configLogging) =>
                   {
                       //configLogging.ClearProviders();
                       configLogging.AddConsole().SetMinimumLevel(LogLevel.Trace);
                   })
                    .ConfigureServices(s =>
                    {
                        s.AddHostedService<StartService>();
                        s.AddSignalR()
                            .AddMessagePackProtocol()
                            .AddJsonProtocol(options =>
                            {
                                // we are running the same tests with JSON and MsgPack protocols and having
                                // consistent casing makes it cleaner to verify results
                                options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver();
                            });
                    })
                    .UseSignalR(c => 
                    {
                        c.MapHub<TestHub>(uri.AbsolutePath);
                    })
                    .Build();
            await host.RunAsync();
        }
    }
}
