using System.Collections.Generic;
using System.IO;
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
            
            var requestMessage = await Parse(sb.ToString());

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
            var requestMessage = await Parse("Host: www.thing.com \r\n");
            
            Assert.Equal("www.thing.com", requestMessage.Headers["Host"]);
        }

        [Fact]
        public async Task ParseRequest_should_reject_trailing_witespace_in_header_names()
        {
            await Assert.ThrowsAnyAsync<BadRequestException>(() => Parse("Host : www.thing.com\r\n"));
        }

        [Fact]
        public async Task ParseRequest_should_reject_obs_fold_headers()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => Parse("first:thing\r\n second:thing\r\n"));
        }

        [Fact]
        public async Task ParseRequest_should_append_duplicate_headers()
        {
            var requestMessage = await Parse("a:1\r\na:2\r\n");
            
            Assert.Equal("1,2", requestMessage.Headers["a"]);
        }

        [Fact]
        public async Task ParseRequest_should_reject_invalid_Content_Length_headers()
        {
            await Assert.ThrowsAsync<BadRequestException>(() => Parse("Content-Length:abc"));
        }
        
        private static Task<RequestMessage> Parse(string streamContent)
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