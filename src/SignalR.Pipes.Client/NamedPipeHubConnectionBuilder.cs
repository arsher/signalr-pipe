using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SignalR.Pipes.Client
{
    /// <summary>
    /// A builder for configuring <see cref="HubConnection"/> instances that will use 
    /// named pipe based transport.
    /// </summary>
    public class NamedPipeHubConnectionBuilder : IHubConnectionBuilder
    {
        private bool hubConnectionBuilt;

        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeHubConnectionBuilder"/> class.
        /// </summary>
        public NamedPipeHubConnectionBuilder()
        {
            Services = new ServiceCollection();
            Services.AddSingleton<HubConnection>();
            Services.AddSingleton<IConnectionFactory, NamedPipeConnectionFactory>();
            Services.AddLogging();
            this.AddJsonProtocol();
        }

        /// <inheritdoc />
        public HubConnection Build()
        {
            // Build can only be used once
            if (hubConnectionBuilt)
            {
                throw new InvalidOperationException("HubConnectionBuilder allows creation only of a single instance of HubConnection.");
            }

            hubConnectionBuilt = true;

            // The service provider is disposed by the HubConnection
            var serviceProvider = Services.BuildServiceProvider();

            var connectionFactory = serviceProvider.GetService<IConnectionFactory>();
            if (connectionFactory == null)
            {
                throw new InvalidOperationException($"Cannot create {nameof(HubConnection)} instance. An {nameof(IConnectionFactory)} was not configured.");
            }

            return serviceProvider.GetService<HubConnection>();
        }

    }
}
