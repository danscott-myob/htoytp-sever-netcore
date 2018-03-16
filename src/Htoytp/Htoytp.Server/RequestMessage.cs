using System;
using System.IO;

namespace Htoytp.Server
{
    internal class RequestMessage : IRequestMessage
    {
        public HttpMethod Method { get; internal set; }
        public string Target { get; internal set; }
        public string HttpVersion { get; internal set; }
        public IMessageHeaders Headers { get; internal set; }
        public Stream Body { get; internal set; }
    }
}