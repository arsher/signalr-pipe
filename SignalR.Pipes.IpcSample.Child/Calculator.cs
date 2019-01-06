using System.Threading.Tasks;
using SignalR.Pipes.IpcSample.Contract;

namespace SignalR.Pipes.IpcSample.Child
{
    public class Calculator : ICalculator
    {
        public Task<int> Add(int one, int two)
        {
            return Task.FromResult(one + two);
        }
    }
}
