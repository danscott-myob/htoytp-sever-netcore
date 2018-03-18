using System.Collections.Generic;

namespace Htoytp.Server
{
    public class MessageHeaders : Dictionary<string, string> 
    {
        public bool HasBody => ContainsKey(StandardHeaders.CONTENT_LENGTH);

        public long ContentLength
            => HasBody
                ? long.Parse(this[StandardHeaders.CONTENT_LENGTH])
                : 0;
    }
}