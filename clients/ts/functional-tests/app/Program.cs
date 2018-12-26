using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

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

            var host = new HostBuilder()
                    .UseConsoleLifetime()
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
                            })
                            .AddHub<TestHub>(url);
                    })
                    .Build();
            await host.RunAsync();
        }
    }
}
