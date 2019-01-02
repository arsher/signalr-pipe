using SignalR.Pipes.Connections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Routing
{
    internal sealed class Router
    {
        private readonly IDictionary<string, Func<NamedPipeConnectionContext, CancellationToken, Task>> routes;

        public Router(IDictionary<string, Func<NamedPipeConnectionContext, CancellationToken, Task>> routes)
        {
            this.routes = routes;
        }

        public async Task RouteAsync(NamedPipeConnectionContext context, CancellationToken cancellationToken)
        {
            if(routes.TryGetValue(context.Name, out var routeHandler))
            {
                await routeHandler(context, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
