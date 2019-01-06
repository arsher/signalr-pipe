# signalr-pipe

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