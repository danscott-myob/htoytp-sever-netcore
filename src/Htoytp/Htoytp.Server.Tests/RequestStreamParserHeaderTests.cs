using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Htoytp.Server.Tests
{
    public class RequestStreamParserHeaderTests
    {
        [Fact]
        public async Task ParseRequest_should_parse_headers()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Host:www.thing.com\r");
            sb.AppendLine("Content-Type:text/plain\r");
            sb.AppendLine("UserAgent:007\r");

            var (requestMessage, error) = await Parse(sb.ToString());

            Assert.Null(error);
            
            var expectedHeaders = new Dictionary<string, string>
            {
                ["Host"] = "www.thing.com",
                ["Content-Type"] = "text/plain",
                ["UserAgent"] = "007",
            };

            Assert.Equal(expectedHeaders, requestMessage.Headers);
        }

        [Fact]
        public async Task ParseRequest_should_ignore_optional_whitespace_in_header_values()
        {
            var (requestMessage, error) = await Parse("Host: www.thing.com \r\n");
            
            Assert.Null(error);

            Assert.Equal("www.thing.com", requestMessage.Headers["Host"]);
        }

        [Fact]
        public async Task ParseRequest_should_reject_trailing_witespace_in_header_names()
        {
            var (_, error) = await Parse("Host : www.thing.com\r\n");

            Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        }

        [Fact]
        public async Task ParseRequest_should_reject_obs_fold_headers()
        {
            var (_, error) = await Parse("first:thing\r\n second:thing\r\n");
            
            Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        }

        [Fact]
        public async Task ParseRequest_should_append_duplicate_headers()
        {
            var (requestMessage, error) = await Parse("a:1\r\na:2\r\n");
            
            Assert.Null(error);

            Assert.Equal("1,2", requestMessage.Headers["a"]);
        }

        [Fact]
        public async Task ParseRequest_should_reject_invalid_Content_Length_headers()
        {
            var (_, error) = await Parse("Content-Length:abc");
            
            Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        }

        private static Task<(RequestMessage request, ResponseMessage error)> Parse(string streamContent)
        {
            IRequestStreamParser parser = new RequestStreamParser();

            var content = $"GET / HTTP/1.1\r\n{streamContent}";

            using (var stream = StringStream(content))
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