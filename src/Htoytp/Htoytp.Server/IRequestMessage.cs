using System;
using System.IO;

namespace Htoytp.Server
{
    public interface IRequestMessage
    {
        HttpMethod Method { get; }
        string Target { get;  }
        string  HttpVersion { get; }
        IMessageHeaders Headers { get; }
        Stream Body { get; }
    }
}