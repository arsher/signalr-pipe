namespace SignalR.Pipes.Ipc
{
    public interface IIpcHubClients<T>
    {
        T Client(string connectionId);
    }
}
