using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Htoytp.Server
{
    public class DefaultRequestProcessor : IRequestProcessor
    {
        private readonly List<IMiddleware> _middlewares;

        public DefaultRequestProcessor()
        {
            _middlewares = new List<IMiddleware>();
        }

        public async Task<ResponseMessage> ProcessAsync(RequestMessage request)
        {
            var response = new ResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
                Headers = new MessageHeaders()
            };

            var context = new MessageContext
            {
                Request = request,
                Response = response,
            };

            var middlewareIndex = 0;

            var t = Task.CompletedTask;

            Task RunMiddleware()
                => middlewareIndex >= _middlewares.Count
                    ? Task.CompletedTask
                    : _middlewares[middlewareIndex++].ProcessAsync(context, RunMiddleware);

            await RunMiddleware();

            return context.Response;
        }

        public IRequestProcessor AddMiddleware(IMiddleware middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }
    }
}