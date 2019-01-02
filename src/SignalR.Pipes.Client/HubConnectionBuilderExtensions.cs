using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Pipes.Common;

namespace SignalR.Pipes.Client
{
    /// <summary>
    /// Extension methods for <see cref="IHubConnectionBuilder"/>.
    /// </summary>
    public static class HubConnectionBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder">The <see cref="IHubConnectionBuilder" /> to configure.</param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IHubConnectionBuilder WithUri(this IHubConnectionBuilder builder, Uri uri)
        {
            PipeUri.Validate(uri);

            builder.Services.Configure<NamedPipeConnectionOptions>(c => c.Uri = uri);

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder">The <see cref="IHubConnectionBuilder" /> to configure.</param>
        /// <param name="uriString"></param>
        /// <returns></returns>
        public static IHubConnectionBuilder WithUri(this IHubConnectionBuilder builder, string uriString)
        {
            var uri = new Uri(uriString);
            return builder.WithUri(uri);
        }
    }
}
