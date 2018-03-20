using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public Task<(RequestMessage requestMessage, ResponseMessage errorResponse)> ParseRequestAsync(
            Stream requestStream)
        {
            var (method, target, version, requestLineError) = ParseRequestLine(requestStream);

            if (requestLineError != null)
            {
                return Task.FromResult<(RequestMessage requestMessage, ResponseMessage errorResponse)>((null,
                    requestLineError));
            }

            var (headers, headerError) = ParseHeaders(requestStream);

            if (headerError != null)
            {
                return Task.FromResult<(RequestMessage requestMessage, ResponseMessage errorResponse)>((null,
                    headerError));
            }

            var requestMessage = new RequestMessage
            {
                Method = method,
                Target = target,
                HttpVersion = version,
                Headers = headers,
                Body = requestStream
            };

            return Task.FromResult<(RequestMessage requestMessage, ResponseMessage errorResponse)>((requestMessage,
                null));
        }

        private static (MessageHeaders headers, ResponseMessage error) ParseHeaders(Stream requestStream)
        {
            string line = null;

            var headers = new MessageHeaders();

            while ((line = ReadLine(requestStream)) != string.Empty)
            {
                var (key, value, error) = SplitHeaderLine(line);

                if (error != null)
                {
                    return (null, error);
                }

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
                return (
                    null,
                    ResponseMessage.BadRequest($"Invalid Content-Length value: '{headers["Content-Length"]}'")
                );
            }

            return (headers, null);
        }

        private static (string key, string value, ResponseMessage error) SplitHeaderLine(string line)
        {
            var parts = line.Split(":");

            var key = parts[0];

            if (key.Trim() != key)
            {
                return (null, null, ResponseMessage.BadRequest(
                    $"Illegal whitespace in header line: '{line}' (no leading/trailing whitespace or obs-fold allowed)"));
            }

            var value = parts[1].Trim();

            return (key, value, null);
        }

        private static (HttpMethod method, string target, string version, ResponseMessage error) ParseRequestLine(
            Stream requestStream)
        {
            var line = ReadLine(requestStream).Split((char) SP);

            var (method, methodError) = ParseHttpMethod(line[0]);

            if (methodError != null)
            {
                return (HttpMethod.Unrecognized, null, null, methodError);
            }
            
            var (target, targetError) = ParseRequestTarget(line[1]);

            if (targetError != null)
            {
                return (HttpMethod.Unrecognized, null, null, targetError);
            }

            return (method, target, ParseHttpVersion(line[2]), null);
        }

        private static string ParseHttpVersion(string http)
        {
            return http.Split('/')[1];
        }

        private static (string target, ResponseMessage error) ParseRequestTarget(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                return (null, ResponseMessage.BadRequest($"Invalid request target"));
            }

            return (target, null);
        }

        private static (HttpMethod method, ResponseMessage error) ParseHttpMethod(string method)
        {
            return RequestMethodLookup.TryGetValue(method, out var requestMethod)
                ? (requestMethod, null)
                : (HttpMethod.Unrecognized, ResponseMessage.BadRequest($"Unknown request method: '{method}'"));
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