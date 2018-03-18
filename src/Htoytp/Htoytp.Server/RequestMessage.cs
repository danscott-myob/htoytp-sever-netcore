using System;
using System.IO;

namespace Htoytp.Server
{
    public class RequestMessage
    {
        public HttpMethod Method { get; set; }
        public string Target { get; set; }
        public string HttpVersion { get; set; }
        public MessageHeaders Headers { get; set; }
        public Stream Body { get; set; }
    }
}