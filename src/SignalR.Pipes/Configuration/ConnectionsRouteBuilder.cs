using Microsoft.AspNetCore.Connections;
using SignalR.Pipes.Routing;
using System;

namespace SignalR.Pipes.Configuration
{
    internal sealed class ConnectionsRouteBuilder
    {
        private readonly IServiceProvider serviceProvider;
        private readonly RouteBuilder routeBuilder;

        public ConnectionsRouteBuilder(IServiceProvider serviceProvider, RouteBuilder routeBuilder)
        {
            this.serviceProvider = serviceProvider;
            this.routeBuilder = routeBuilder;
        }

        public void MapConnections(string name, Action<IConnectionBuilder> configure)
        {
            var connectionBuilder = new ConnectionBuilder(serviceProvider);
            configure(connectionBuilder);
            var connectionDelegate = connectionBuilder.Build();

            routeBuilder.MapRoute(name, (c, token) => connectionDelegate(c));
        }
    }
}
