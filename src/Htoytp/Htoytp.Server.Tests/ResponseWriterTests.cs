using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Htoytp.Server.Tests
{
    public class ResponseWriterTests
    {
        [Fact]
        public async Task TranslateResponse_should_write_the_status_line()
        {
            var response = new ResponseMessage
            {
                Headers = new MessageHeaders(),
                StatusCode = HttpStatusCode.PartialContent
            };

            var responseString = await Translate(response);

            Assert.StartsWith("HTTP/1.1 206 PartialContent\r\n", responseString);
        }

        [Fact]
        public async Task TranslateResponse_should_add_all_headers()
        {
            var response = new ResponseMessage
            {
                Headers = new MessageHeaders
                {
                    ["Content-Type"] = "text/plain",
                    ["Accepts"] = "stuff",
                    ["Content-Encoding"] = "none",
                },
                StatusCode = HttpStatusCode.Accepted
            };

            var responseString = await Translate(response);

            var parts = responseString.Split("\r\n");

            Assert.Equal("Content-Type:text/plain", parts[1]);
            Assert.Equal("Accepts:stuff", parts[2]);
            Assert.Equal("Content-Encoding:none", parts[3]);
        }

        private Task<string> Translate(ResponseMessage response)
        {
            var writer = new ResponseWriter();

            return writer.TranslateResponse(response);
        }
    }
}