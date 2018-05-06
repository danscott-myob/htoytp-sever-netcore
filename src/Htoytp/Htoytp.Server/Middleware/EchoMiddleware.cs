using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Htoytp.Server.Middleware
{
    public class EchoMiddleware : IMiddleware
    {
        public Task ProcessAsync(MessageContext context,
            Func<Task> nextAsync)
        {
            context.Response.Headers = context.Request.Headers;
            
            context.Response.StatusCode = HttpStatusCode.OK;

            context.Response.Body = "{\"thing\":\"what\"}";

            return Task.FromResult(context);
        }
    }
}