# signalr-pipe

[![Build status](https://ci.appveyor.com/api/projects/status/cm01wuq8gul5h148/branch/master?svg=true)](https://ci.appveyor.com/project/SerfzDvid/signalr-pipe/branch/master)

## Purpose
This is an experimental lib to make use of the connection abstractions in the ASP.NET Core version of SignalR. It aims to implement a named pipe based transport layer to be used as a form of IPC between .NET and/or Node processes. 

## How to use
This SignalR extension is intended to be used with the [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host).

### Example setup for the server:

```csharp
var host = new HostBuilder()
    .UseHostUri(new Uri("signalr.pipe://testhost/"))
    .UseSignalR(b =>
    {
        b.MapHub<TestHub>("/testpath/net");
    })
    .ConfigureServices(collection =>
    {
        collection.AddSignalR();
    })
    .Build();

host.Start();
```

### Example client

#### .NET

```csharp
var connection = new NamedPipeHubConnectionBuilder()
    .WithUri("signalr.pipe://testhost/testpath/net")
    .Build();

await connection.StartAsync();
```

#### NodeJS

```javascript
var connection = new NamedPipeHubConnectionBuilder()
                .WithUri("signalr.pipe://testhost/testpath/net")
                .Build();

await connection.StartAsync();
```

## Releases

### CI Builds

Every commit on master creates a new set of packages:

- [![](https://img.shields.io/npm/v/signalr-pipe/latest.svg?registry_uri=https%3A%2F%2Fwww.myget.org%2FF%2Fdserfozo%2Fnpm%2F&label=signalr-pipe@node)](https://www.myget.org/feed/dserfozo/package/npm/signalr-pipe)
- [![](https://img.shields.io/myget/dserfozo/vpre/Signalr.Pipes.Common.svg?label=SignalR.Pipes.Common)](https://www.myget.org/feed/dserfozo/package/nuget/SignalR.Pipes.Common)
- [![](https://img.shields.io/myget/dserfozo/vpre/Signalr.Pipes.svg?label=SignalR.Pipes)](https://www.myget.org/feed/dserfozo/package/nuget/SignalR.Pipes)
- [![](https://img.shields.io/myget/dserfozo/vpre/Signalr.Pipes.Client.svg?label=SignalR.Pipes.Client)](https://www.myget.org/feed/dserfozo/package/nuget/SignalR.Pipes.Client)

### Stable

There is no stable release at this point.