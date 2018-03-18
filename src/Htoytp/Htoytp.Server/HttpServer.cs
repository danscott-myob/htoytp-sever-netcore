using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Htoytp.Server.Middleware;
using static System.Text.Encoding;

namespace Htoytp.Server
{
    public class HttpServer
    {
        private List<IMiddleware> _middlewares = new List<IMiddleware>();

        public HttpServer(int port)
        {
            _port = port;
        }

        private readonly int _port;

        public async Task Start()
        {
            TcpListener server = null;

            var requestStreamParser = new RequestStreamParser();
            
            var requestProcessor = new DefaultRequestProcessor();

            requestProcessor.AddMiddleware(new EchoMiddleware());
            
            try
            {
                server = new TcpListener(IPAddress.Parse("127.0.0.1"), _port);

                server.Start();

                while (true)
                {
                    using (var client = await server.AcceptTcpClientAsync())
                    {
                        var stream = client.GetStream();

                        var responseString = "HTTP/1.1 200 OK";

                        try
                        {
                            var requestMessage = await requestStreamParser.ParseRequestAsync(stream);

                            var responseMessage = await requestProcessor.ProcessAsync(requestMessage);

                            var responseLines = new List<string>();
                            
                            responseLines.Add($"HTTP/1.1 {(int)responseMessage.StatusCode} {responseMessage.StatusCode}");
                            responseLines.AddRange(responseMessage.Headers.Select(header => $"{header.Key}:{header.Value}"));
                            responseLines.Add(string.Empty);

                            responseString = string.Join("\r\n", responseLines);
                        }
                        catch (BadRequestException ex)
                        {
                            var cl = ASCII.GetBytes(ex.Message).Length;
                            responseString = $"HTTP/1.1 401 BadRequest\r\nContent-Length:{cl}\r\n\r\n{ex.Message}";
                        }
                        catch (Exception ex)
                        {
                            var cl = ASCII.GetBytes(ex.Message);
                            responseString =
                                $"HTTP/1.1 500 InternalServerError\r\nContent-Length:{cl}\r\n\r\n{ex.Message}";
                        }
                        
                        var response = ASCII.GetBytes(responseString);

                        await stream.WriteAsync(response, 0, response.Length);

                        client.Close();
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server?.Stop();
            }
        }
    }
}