using Microsoft.AspNetCore.SignalR;
using SignalR.Pipes.Ipc;
using SignalR.Pipes.IpcSample.Contract;

namespace SignalR.Pipes.IpcSample
{
    public class CalculatorHub : IpcHub<ICalculator>
    {
        private readonly IChildProcessManager processManager;

        public CalculatorHub(IChildProcessManager processManager)
        {
            this.processManager = processManager;
        }
    }
}
