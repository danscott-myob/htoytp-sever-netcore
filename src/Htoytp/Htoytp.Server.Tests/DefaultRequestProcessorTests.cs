using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using Xunit;

namespace Htoytp.Server.Tests
{
    public class DefaultRequestProcessorTests
    {
        [Fact]
        public async Task ProcessAsync_should_return_a_default_message()
        {
            var processor = new DefaultRequestProcessor();

            var requestMessage = new RequestMessage();


            var response = await processor.ProcessAsync(requestMessage);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Null(response.Body);
            Assert.Empty(response.Headers);
        }

        [Fact]
        public async Task ProcessAsync_should_run_provided_middleware()
        {
            var mockMiddleware = new Mock<IMiddleware>();

            var requestMessage = new RequestMessage();

            var callCount = 0;

            mockMiddleware
                .Setup(x => x.ProcessAsync(It.IsAny<MessageContext>(),
                    It.IsAny<Func<Task>>()))
                .Returns((MessageContext c, Func<Task> n) =>
                {
                    callCount++;
                    return n();
                });

            var processor = new DefaultRequestProcessor().AddMiddleware(mockMiddleware.Object);

            await processor.ProcessAsync(requestMessage);
            
            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task ProcessAsync_should_run_all_middlewares_in_order()
        {
            var mockMiddleware1 = new Mock<IMiddleware>();
            var mockMiddleware2 = new Mock<IMiddleware>();

            var calls = new List<string>();

            mockMiddleware1
                .Setup(x => x.ProcessAsync(It.IsAny<MessageContext>(), It.IsAny<Func<Task>>()))
                .Returns((MessageContext c, Func<Task> n) =>
                {
                    calls.Add("middleware1");
                    return n();
                });

            mockMiddleware2
                .Setup(x => x.ProcessAsync(It.IsAny<MessageContext>(), It.IsAny<Func<Task>>()))
                .Returns((MessageContext c, Func<Task> n) =>
                {
                    calls.Add("middleware2");
                    return n();
                });

            var processor = new DefaultRequestProcessor()
                .AddMiddleware(mockMiddleware1.Object)
                .AddMiddleware(mockMiddleware2.Object);

            await processor.ProcessAsync(new RequestMessage());
            
            Assert.Equal(new [] { "middleware1", "middleware2"}, calls);

        }
    }
}