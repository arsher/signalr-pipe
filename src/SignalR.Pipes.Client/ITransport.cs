using System.IO.Pipelines;
using System.Threading.Tasks;

namespace SignalR.Pipes.Client
{
    public interface ITransport : IDuplexPipe
    {
        Task<string> StartAsync();

        Task StopAsync();
    }
}
