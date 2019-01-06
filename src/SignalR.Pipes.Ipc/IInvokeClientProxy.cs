using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalR.Pipes.Ipc
{
    /// <summary>
    /// 
    /// </summary>
    public interface IInvokeClientProxy
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="returnType"></param>
        /// <param name="args"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<object> InvokeCoreAsync(string method, Type returnType, object[] args, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InvokeCoreAsync(string method, object[] args, CancellationToken cancellationToken = default);
    }
}
