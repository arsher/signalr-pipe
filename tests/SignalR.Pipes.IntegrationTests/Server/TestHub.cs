using Microsoft.AspNetCore.SignalR;
using SignalR.Pipes.IntegrationTests.Contract;

namespace SignalR.Pipes.IntegrationTests.Server
{
    public class TestHub : Hub<IClient>
    {
        public string Echo(string data)
        {
            return data;
        }
    }
}
