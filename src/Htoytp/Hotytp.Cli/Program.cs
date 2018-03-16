using System;
using System.Linq;
using Htoytp.Server;

namespace Hotytp.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = 8080;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-p")
                {
                    port = int.Parse(args[++i]);
                }
            }

            var server = new HttpServer(port);
            
            server.Start().Wait();
        }
    }
}