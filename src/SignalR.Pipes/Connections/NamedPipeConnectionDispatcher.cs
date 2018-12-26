using Microsoft.AspNetCore.Connections;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR.Pipes.Connections
{
    public class NamedPipeConnectionDispatcher
    {
        private static readonly RNGCryptoServiceProvider keyGenerator = new RNGCryptoServiceProvider();

        public async Task ExecuteAsync(NamedPipeContext context, ConnectionDelegate connectionDelegate, CancellationToken token)
        {
            var id = MakeNewConnectionId();
            var namedPipeConnectionContext = new NamedPipeConnectionContext(id, context.Stream);

            await namedPipeConnectionContext.StartAsync().ConfigureAwait(false);
            await connectionDelegate.Invoke(namedPipeConnectionContext).ConfigureAwait(false);

            Console.WriteLine();
        }

        private static string MakeNewConnectionId()
        {
            var buffer = new byte[16];
            keyGenerator.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }
    }
}
