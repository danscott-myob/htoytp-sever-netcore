using System.Net;

namespace Htoytp.Server
{
    public class ResponseMessage
    {
        public HttpStatusCode StatusCode { get; set; }
        public MessageHeaders Headers { get; set; }
        public object Body { get; set; }


        public static ResponseMessage BadRequest(string message)
        {
            return new ResponseMessage
            {
                Body = message,
                Headers = new MessageHeaders(),
                StatusCode = HttpStatusCode.BadRequest,
            };
        }
    }

    public class MessageContext
    {
        public RequestMessage Request { get; set; }
        public ResponseMessage Response { get; set; }
    }
}