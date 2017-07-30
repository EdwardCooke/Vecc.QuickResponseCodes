using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace Vecc.QuickResponseCodes.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hostName = Dns.GetHostName();
            Console.WriteLine("Host: {0}", hostName);
            Console.WriteLine("IP: {0}", string.Join(", ", Dns.GetHostEntry(hostName).AddressList.Select(x => x.ToString())));

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();
            host.Run();
        }
    }
}
