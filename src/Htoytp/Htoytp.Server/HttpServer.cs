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
            
            var responseWriter = new ResponseWriter();
            
            try
            {
                server = new TcpListener(IPAddress.Parse("127.0.0.1"), _port);

                server.Start();

                while (true)
                {
                    using (var client = await server.AcceptTcpClientAsync())
                    {
                        var stream = client.GetStream();

                        ResponseMessage responseMessage;

                        try
                        {
                            var (requestMessage, error) = await requestStreamParser.ParseRequestAsync(stream);

                            responseMessage = error ?? await requestProcessor.ProcessAsync(requestMessage);
                        }
                        catch (Exception ex)
                        {
                            responseMessage = new ResponseMessage
                            {
                                Body = ex.Message,
                                Headers = new MessageHeaders(),
                                StatusCode = HttpStatusCode.InternalServerError,
                            };
                        }

                        var responseString = await responseWriter.TranslateResponse(responseMessage);
                        
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