using System.Net;

namespace Htoytp.Server
{
    public class ResponseMessage
    {
        public HttpStatusCode StatusCode { get; set; }
        public MessageHeaders Headers { get; set; }
        public object Body { get; set; }
    }

    public class MessageContext
    {
        public RequestMessage Request { get; set; }
        public ResponseMessage Response { get; set; }
    }
}