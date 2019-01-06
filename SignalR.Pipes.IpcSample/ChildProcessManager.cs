using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SignalR.Pipes.IpcSample.Contract;
using SignalR.Pipes.Ipc;

namespace SignalR.Pipes.IpcSample
{
    public class ChildProcessManager : IChildProcessManager
    {
        private readonly IDictionary<int, ChildProcess> processes = new Dictionary<int, ChildProcess>();

        public IReadOnlyDictionary<int, ChildProcess> Processes => new ReadOnlyDictionary<int, ChildProcess>(processes);

        public async Task RunChildProcess()
        {
            var uri = new Uri($"signalr.pipe://{Guid.NewGuid():N}/");
            var host = new HostBuilder()
                .UseHostUri(uri)
                .ConfigureLogging(config => { config.AddConsole().SetMinimumLevel(LogLevel.Trace); })
                .UseSignalR(b =>
                {
                    b.MapHub<CalculatorHub>("/ipc");
                })
                .ConfigureServices(collection =>
                {
                    collection.AddSignalR()
                        .AddIpc();
                    collection.AddSingleton<IChildProcessManager>(this);
                })
                .Build();

            await host.StartAsync();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo("SignalR.Pipes.IpcSample.Child.exe")
                {
                    Arguments = $"--url \"{uri}ipc\""
                }
            };

            process.Start();

            processes.Add(process.Id, new ChildProcess(host));
        }

        public async Task<int[]> RunCalcOnAll(int one, int two)
        {
            var result = new List<int>();
            foreach (var processesValue in processes.Values)
            {
                var hubContext = processesValue.Host.Services.GetRequiredService<IHubContext<CalculatorHub>>();
                hubContext.SingleClient();
                //result.Add(await hubContext.Clients.All.Add(one, two));
                await Task.Delay(100);
            }

            return result.ToArray();
        }
    }
}
