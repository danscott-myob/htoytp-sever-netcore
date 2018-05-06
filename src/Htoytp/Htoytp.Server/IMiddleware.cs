using System;
using System.Threading.Tasks;

namespace Htoytp.Server
{
    public interface IMiddleware
    {
        Task ProcessAsync(MessageContext context, Func<Task> nextAsync);
    }
}