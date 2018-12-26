using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using SignalR.Pipes.Common;

namespace SignalR.Pipes.Client
{
    public static class HubConnectionBuilderExtensions
    {
        public static IHubConnectionBuilder WithUri(this IHubConnectionBuilder builder, Uri uri)
        {
            PipeUri.Validate(uri);

            builder.Services.Configure<NamedPipeConnectionOptions>(c => c.Uri = uri);

            return builder;
        }

        public static IHubConnectionBuilder WithUri(this IHubConnectionBuilder builder, string uriString)
        {
            var uri = new Uri(uriString);
            return builder.WithUri(uri);
        }
    }
}
