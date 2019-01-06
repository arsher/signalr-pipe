using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Pipes.Client;

namespace SignalR.Pipes.IpcSample.Child
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async Task MainAsync(string[] args)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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

            var connection = new NamedPipeHubConnectionBuilder()
                .WithUri(url)
                .Build();

            //connection.On<int, int, Task<int>>("Add", new Func<int, int, Task<int>>((i, i1) => Task.FromResult(i + i1)));

            //connection.On("Add", new[]{typeof(int), typeof(int)}, )

            await connection.StartAsync();

            Console.WriteLine("Connected");

            Console.ReadLine();
        }
    }
}
