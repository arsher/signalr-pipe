using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SignalR.Pipes.Common;

namespace SignalR.Pipes.Configuration
{
    public class HubRouteOptions
    {
        private readonly IDictionary<Uri, Action<IConnectionBuilder>> hubMap = new Dictionary<Uri, Action<IConnectionBuilder>>();

        public IReadOnlyDictionary<Uri, Action<IConnectionBuilder>> HubMap => new ReadOnlyDictionary<Uri, Action<IConnectionBuilder>>(hubMap);

        public void AddHub(Uri uri, Action<IConnectionBuilder> configure)
        {
            PipeUri.Validate(uri);

            hubMap.Add(uri, configure);
        }
    }
}
