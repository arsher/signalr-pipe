using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.Pipes.IntegrationTests.Server
{
    public class DisposableLifetimeOptions
    {
        public Action<IDisposable> DisposableSetter { get; set; }
    }
}
