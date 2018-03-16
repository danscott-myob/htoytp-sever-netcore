using System.Collections.Generic;

namespace Htoytp.Server
{
    public interface IMessageHeaders : IReadOnlyDictionary<string, string>
    {
        bool HasBody { get; }
        long ContentLength { get; }
    }

    internal class MessageHeaders : Dictionary<string, string>, IMessageHeaders
    {
        public bool HasBody => ContainsKey(StandardHeaders.CONTENT_LENGTH);

        public long ContentLength
            => HasBody
                ? long.Parse(this[StandardHeaders.CONTENT_LENGTH])
                : 0;
    }
}