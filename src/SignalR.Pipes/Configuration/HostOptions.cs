using System;

namespace SignalR.Pipes.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class HostOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Action<HubRouteBuilder> Configure { get; set; }
    }
}
