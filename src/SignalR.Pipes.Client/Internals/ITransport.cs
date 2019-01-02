using System.IO.Pipelines;
using System.Threading.Tasks;

namespace SignalR.Pipes.Client
{
    internal interface ITransport : IDuplexPipe
    {
        Task<string> StartAsync();

        Task StopAsync();
    }
}
