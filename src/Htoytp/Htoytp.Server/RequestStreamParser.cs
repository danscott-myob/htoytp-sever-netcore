using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Htoytp.Server
{
    public class RequestStreamParser : IRequestStreamParser
    {
        private const int SP = ' ';
        private const int CR = '\r';
        private const int LF = '\n';

        private static readonly Dictionary<string, HttpMethod> RequestMethodLookup
            = new Dictionary<string, HttpMethod>
            {
                ["CONNECT"] = HttpMethod.Connect,
                ["DELETE"] = HttpMethod.Delete,
                ["GET"] = HttpMethod.Get,
                ["HEAD"] = HttpMethod.Head,
                ["OPTIONS"] = HttpMethod.Options,
                ["POST"] = HttpMethod.Post,
                ["PUT"] = HttpMethod.Put,
                ["TRACE"] = HttpMethod.Trace,
            };

        public Task<RequestMessage> ParseRequestAsync(Stream requestStream)
        {
            var (method, target, version) = ParseRequestLine(requestStream);

            var headers = ParseHeaders(requestStream);

            var requestMessage = new RequestMessage
            {
                Method = method,
                Target = target,
                HttpVersion = version,
                Headers = headers,
                Body = requestStream
            };

            return Task.FromResult(requestMessage);
        }

        private static MessageHeaders ParseHeaders(Stream requestStream)
        {
            string line = null;
            
            var headers = new MessageHeaders();

            while ((line = ReadLine(requestStream)) != string.Empty)
            {
                var (key, value) = SplitHeaderLine(line);

                if (headers.ContainsKey(key))
                {
                    headers[key] = $"{headers[key]},{value}";
                }
                else
                {
                    headers.Add(key, value);
                }
            }

            if (headers.ContainsKey("Content-Length") && long.TryParse(headers["Content-Length"], out var _) == false)
            {
                throw new BadRequestException($"Invalid Content-Length value: '{headers["Content-Length"]}'");
            }

            return headers;
        }

        private static (string key, string value) SplitHeaderLine(string line)
        {
            var parts = line.Split(":");

            var key = parts[0];

            if (key.Trim() != key)
            {
                throw new BadRequestException($"Illegal whitespace in header line: '{line}' (no leading/trailing whitespace or obs-fold allowed)");
            }

            var value = parts[1].Trim();
            
            return (key, value);
        }

        private static (HttpMethod method, string target, string version) ParseRequestLine(Stream requestStream)
        {
            var line = ReadLine(requestStream).Split((char) SP);

            return (ParseHttpMethod(line[0]), ParseRequestTarget(line[1]), ParseHttpVersion(line[2]));
        }

        private static string ParseHttpVersion(string http)
        {
            return http.Split('/')[1];
        }

        private static string ParseRequestTarget(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                throw new BadRequestException($"Invalid request target");
            }

            return target;
        }

        private static HttpMethod ParseHttpMethod(string method)
        {
            if (RequestMethodLookup.TryGetValue(method, out var requestMethod))
            {
                return requestMethod;
            }

            throw new BadRequestException($"Unknown request method: '{method}'");
        }

        private static string ReadLine(Stream requestStream)
        {
            var buffer = new byte[1];
            var done = false;
            var readBytes = new List<byte>();
            while (!done && requestStream.Read(buffer, 0, 1) != 0)
            {
                var currentOctet = buffer[0];
                if (currentOctet == CR)
                {
                    var nextOctet = requestStream.ReadByte();
                    if (nextOctet == LF)
                    {
                        done = true;
                    }
                }
                else
                {
                    readBytes.Add(currentOctet);
                }
            }

            return Encoding.ASCII.GetString(readBytes.ToArray());
        }
    }
}