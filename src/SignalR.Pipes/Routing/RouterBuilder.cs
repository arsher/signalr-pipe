using SignalR.Pipes.Connections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Routing
{
    internal sealed class RouteBuilder
    {
        private readonly Dictionary<string, Func<NamedPipeConnectionContext, CancellationToken, Task>> routes = 
            new Dictionary<string, Func<NamedPipeConnectionContext, CancellationToken, Task>>();

        public RouteBuilder MapRoute(string name, Func<NamedPipeConnectionContext, CancellationToken, Task> handler)
        {
            routes.Add(name, handler);
            return this;
        }

        public Func<NamedPipeConnectionContext, CancellationToken, Task> Build()
        {
            var router = new Router(routes);
            return router.RouteAsync;
        }
    }
}
