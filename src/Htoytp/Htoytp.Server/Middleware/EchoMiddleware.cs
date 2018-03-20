using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Htoytp.Server.Middleware
{
    public class EchoMiddleware : IMiddleware
    {
        public Task<MessageContext> ProcessAsync(MessageContext context,
            Func<MessageContext, Task<MessageContext>> nextAsync)
        {
            context.Response.Headers = context.Request.Headers;
            
            context.Response.StatusCode = HttpStatusCode.OK;

            return Task.FromResult(context);
        }
    }
}