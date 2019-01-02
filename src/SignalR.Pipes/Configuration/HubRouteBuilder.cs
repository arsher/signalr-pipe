using Microsoft.AspNetCore.SignalR;

namespace SignalR.Pipes.Configuration
{
    /// <summary>
    /// Maps incoming requests to <see cref="Hub"/> types.
    /// </summary>
    public sealed class HubRouteBuilder
    {
        private readonly ConnectionsRouteBuilder connectionsRouteBuilder;

        internal HubRouteBuilder(ConnectionsRouteBuilder connectionsRouteBuilder)
        {
            this.connectionsRouteBuilder = connectionsRouteBuilder;
        }

        /// <summary>
        /// Maps incoming requests with the specified name/path to the specified <see cref="Hub"/> type.
        /// </summary>
        public void MapHub<THub>(string name) where THub : Hub
        {
            connectionsRouteBuilder.MapConnections(name, c =>
            {
                c.UseHub<THub>();
            });
        }
    }
}
