using Microsoft.Extensions.Hosting;
using System;
using SignalR.Pipes.Configuration;
using SignalR.Pipes.Common;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IHostBuilder"/>.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IHostBuilder UseHostUri(this IHostBuilder @this, Uri uri)
        {
            PipeUri.Validate(uri);

            @this.ConfigureServices((_, collection) => collection.Configure<HostOptions>(options => options.Uri = uri));

            return @this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IHostBuilder UseSignalR(this IHostBuilder @this, Action<HubRouteBuilder> configure)
        {
            @this.ConfigureServices((_, collection) => collection.Configure<HostOptions>(options => options.Configure = configure));

            return @this;
        }
    }
}
