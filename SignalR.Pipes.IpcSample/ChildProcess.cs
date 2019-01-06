using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace SignalR.Pipes.IpcSample
{
    public class ChildProcess
    {
        public IHost Host { get; }

        public ChildProcess(IHost host)
        {
            Host = host;
        }
    }
}
