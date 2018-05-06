using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Htoytp.Server
{
    public class ResponseWriter
    {
        private const string CRLF = "\r\n";

        public Task<string> TranslateResponse(ResponseMessage response)
        {
            var lines = new List<string>
            {
                $"HTTP/1.1 {(int) response.StatusCode} {response.StatusCode}"
            };

            lines.AddRange(response.Headers.Select(kv => $"{kv.Key}:{kv.Value}"));

            lines.Add(string.Empty);
            
            if (response.Body is string s)
            {
                lines.Insert(lines.Count - 2, $"Content-Length:{s.Length}");
                lines.Add(s);
            }
            
            return Task.FromResult(string.Join(CRLF, lines));
        }
    }
}