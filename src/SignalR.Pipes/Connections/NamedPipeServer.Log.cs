using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.Pipes.Connections
{
    internal partial class NamedPipeServer
    {
        private static class Log
        {
            private static readonly Action<ILogger, Exception> errorDuringClientProcess = 
                LoggerMessage.Define(LogLevel.Error, new EventId(1, "ErrorProcessingClient"), "Error while processing named pipe client.");

            private static readonly Action<ILogger, Exception> clientConnected =
                LoggerMessage.Define(LogLevel.Trace, new EventId(2, "ClientConnected"), "New Client Connected");

            private static readonly Action<ILogger, Exception> clientDisconnectedBeforeHandshake =
                LoggerMessage.Define(LogLevel.Error, new EventId(3, "ClientDisconnectedBeforeHandshake"), "Client closed before sending actual pipe name.");

            private static readonly Action<ILogger, Exception> disconnectingNegotiator =
                LoggerMessage.Define(LogLevel.Trace, new EventId(4, "DisconnectingNegotiator"), "Disconneting the negotiator pipe.");

            private static readonly Action<ILogger, Exception> startingClientConnection =
                LoggerMessage.Define(LogLevel.Trace, new EventId(5, "StartingClientConnection"), "Starting client connection.");

            private static readonly Action<ILogger, Exception> errorStartingClientConnection =
                LoggerMessage.Define(LogLevel.Error, new EventId(6, "ErrorStartingClient"), "Error while processing a client connection.");

            private static readonly Action<ILogger, Exception> errorAcceptorLoop =
                LoggerMessage.Define(LogLevel.Error, new EventId(7, "ErrorInAcceptorLoop"), "Error while accepting client connections.");

            public static void ErrorDuringClientProcess(ILogger logger, Exception e)
            {
                errorDuringClientProcess(logger, e);
            }

            public static void ClientConnected(ILogger logger)
            {
                clientConnected(logger, null);
            }

            public static void ClientDisconnectedBeforeHandshake(ILogger logger)
            {
                clientDisconnectedBeforeHandshake(logger, null);
            }

            public static void DisconnectingNegotiator(ILogger logger)
            {
                disconnectingNegotiator(logger, null);
            }

            public static void StartingClientConnection(ILogger logger)
            {
                startingClientConnection(logger, null);
            }

            public static void ErrorWhileStartingClientConnection(ILogger logger, Exception e)
            {
                errorStartingClientConnection(logger, e);
            }

            public static void LoopError(ILogger logger, Exception e)
            {
                errorAcceptorLoop(logger, e);
            }
        }
    }
}
