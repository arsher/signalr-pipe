using System.Threading.Tasks;

namespace SignalR.Pipes.IpcSample.Contract
{
    public interface ICalculator
    {
        Task<int> Add(int one, int two);
    }
}
