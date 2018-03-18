using System;
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
            var fakeResponse = new ResponseMessage();

            mockMiddleware
                .Setup(x => x.ProcessAsync(It.IsAny<MessageContext>(),
                    It.IsAny<Func<MessageContext, Task<MessageContext>>>()))
                .ReturnsAsync(new MessageContext
                {
                    Request = new RequestMessage(),
                    Response = fakeResponse,
                });

            var processor = new DefaultRequestProcessor().AddMiddleware(mockMiddleware.Object);

            var response = await processor.ProcessAsync(requestMessage);

            Assert.Same(fakeResponse, response);
        }

        [Fact]
        public async Task ProcessAsync_should_run_all_middlewares_in_order()
        {
            var mockMiddleware1 = new Mock<IMiddleware>();
            var mockMiddleware2 = new Mock<IMiddleware>();

            var mockContext1 = new MessageContext();
            var mockContext2 = new MessageContext {Response = new ResponseMessage()};
            
            mockMiddleware1
                .Setup(x => x.ProcessAsync(It.IsAny<MessageContext>(), It.IsAny<Func<MessageContext, Task<MessageContext>>>()))
                .Returns((MessageContext c, Func<MessageContext, Task<MessageContext>> n) => n(mockContext1));

            mockMiddleware2
                .Setup(x => x.ProcessAsync(It.IsAny<MessageContext>(),
                    It.IsAny<Func<MessageContext, Task<MessageContext>>>())).ReturnsAsync(mockContext2);

            var processor = new DefaultRequestProcessor()
                .AddMiddleware(mockMiddleware1.Object)
                .AddMiddleware(mockMiddleware2.Object);

            var response = await processor.ProcessAsync(new RequestMessage());

            Assert.Same(mockContext2.Response, response);

            mockMiddleware2.Verify(x => x.ProcessAsync(mockContext1,
                It.IsAny<Func<MessageContext, Task<MessageContext>>>()));
        }
    }
}