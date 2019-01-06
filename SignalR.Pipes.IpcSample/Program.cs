using System;
using System.Threading.Tasks;
using Autofac;

namespace SignalR.Pipes.IpcSample
{
    class Program
    {


        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<ChildProcessManager>()
                .As<IChildProcessManager>()
                .SingleInstance();
            var container = containerBuilder.Build();

            while (true)
            {
                var command = await Console.In.ReadLineAsync();

                if (command == "exit")
                {
                    Environment.Exit(0);
                }
                else if (command == "start")
                {
                    await container.Resolve<IChildProcessManager>().RunChildProcess();
                }
                else if (command == "calc")
                {
                    var list = await container.Resolve<IChildProcessManager>().RunCalcOnAll(1, 2);
                    foreach (var i in list)
                    {
                        Console.WriteLine(i);
                    }
                }
            }
        }
    }
}
