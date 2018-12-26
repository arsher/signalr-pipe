using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace SignalR.Pipes.IntegrationTests.Server
{
    public class DisposableLifetime : IHostLifetime
    {
        private class Disposable : IDisposable
        {
            private readonly IApplicationLifetime applicationLifetime;
            private bool disposed;

            public Disposable(IApplicationLifetime applicationLifetime)
            {
                this.applicationLifetime = applicationLifetime;
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    var doneEvent = new ManualResetEvent(false);
                    using (applicationLifetime.ApplicationStopped.Register((() => doneEvent.Set())))
                    {
                        applicationLifetime.StopApplication();
                        doneEvent.WaitOne();
                    }
                }
            }
        }

        private IApplicationLifetime ApplicationLifetime { get; }

        private DisposableLifetimeOptions Options { get; }

        public DisposableLifetime(IOptions<DisposableLifetimeOptions> options, IApplicationLifetime applicationLifetime)
        {
            Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            Options.DisposableSetter(new Disposable(ApplicationLifetime));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
