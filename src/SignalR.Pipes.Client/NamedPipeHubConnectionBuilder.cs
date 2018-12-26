using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SignalR.Pipes.Client
{
    public class NamedPipeHubConnectionBuilder : IHubConnectionBuilder
    {
        private bool hubConnectionBuilt;

        public IServiceCollection Services { get; }

        public NamedPipeHubConnectionBuilder()
        {
            Services = new ServiceCollection();
            Services.AddSingleton<HubConnection>();
            Services.AddSingleton<IConnectionFactory, NamedPipeConnectionFactory>();
            Services.AddLogging();
            this.AddJsonProtocol();
        }

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
