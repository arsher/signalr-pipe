using System.Threading.Tasks;

namespace SignalR.Pipes.Ipc
{
    public static class InvokeClientProxyExtensions
    {
        public static async Task<T> InvokeAsync<T>(this IInvokeClientProxy @this, string methodName, object[] args)
        {
            return (T)await @this.InvokeCoreAsync(methodName, typeof(T), args).ConfigureAwait(false);
        }
    }
}
