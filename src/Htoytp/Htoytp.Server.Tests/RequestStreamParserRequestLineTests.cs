using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Htoytp.Server.Tests
{
    public class RequestStreamParserRequestLineTests
    {
        [Theory]
        [InlineData("CONNECT", HttpMethod.Connect)]
        [InlineData("DELETE", HttpMethod.Delete)]
        [InlineData("GET", HttpMethod.Get)]
        [InlineData("HEAD", HttpMethod.Head)]
        [InlineData("OPTIONS", HttpMethod.Options)]
        [InlineData("POST", HttpMethod.Post)]
        [InlineData("PUT", HttpMethod.Put)]
        [InlineData("TRACE", HttpMethod.Trace)]
        public async Task ParseRequest_should_read_request_type_from_request_line(string headerMethod, HttpMethod method)
        {
            var requestMessage = await Parse($"{headerMethod} / HTTP/1.1\r\n");

            Assert.Equal(method, requestMessage.Method);
        }

        [Fact]
        public async Task ParseRequest_should_throw_BadRequestException_for_unknown_request_methods()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => Parse("Get / HTTP/1.1\r\n"));
        }

        [Fact]
        public async Task ParseRequest_should_read_request_target_from_request_line()
        {
            const string requestTarget = "/whatever/stuff?thing=ok#at";

            var requestMessage = await Parse($"GET {requestTarget} HTTP/1.1");

            Assert.Equal(requestTarget, requestMessage.Target);
        }

        [Fact]
        public async Task ParseRequest_should_throw_BadRequestException_for_invalid_target_urls()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => Parse("GET  HTTP/1.1"));
        }

        [Fact]
        public async Task ParseRequest_should_read_http_version()
        {
            var requestMessage = await Parse("GET /a/b/c HTTP/1.1");
            
            Assert.Equal("1.1", requestMessage.HttpVersion);
        }


        private static Task<IRequestMessage> Parse(string streamContent)
        {
            IRequestStreamParser parser = new RequestStreamParser();

            using (var stream = StringStream(streamContent))
            {
                return parser.ParseRequestAsync(stream);
            }
        }

        private static Stream StringStream(string streamContent)
        {
            return new MemoryStream(Encoding.ASCII.GetBytes(streamContent));
        }
    }
}