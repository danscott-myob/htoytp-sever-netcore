using System;
using System.Threading.Tasks;

namespace Htoytp.Server
{
    public interface IMiddleware
    {
        Task<MessageContext> ProcessAsync(MessageContext context, Func<MessageContext, Task<MessageContext>> nextAsync);
    }
}