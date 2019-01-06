using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalR.Pipes.IpcSample
{
    public interface IChildProcessManager
    {
        IReadOnlyDictionary<int, ChildProcess> Processes { get; }
        Task RunChildProcess();
        Task<int[]> RunCalcOnAll(int one, int two);
    }
}
