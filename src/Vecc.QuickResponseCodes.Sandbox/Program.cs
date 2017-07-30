using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Vecc.QuickResponseCodes.Abstractions;

namespace Vecc.QuickResponseCodes.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddQuickResponseCodes();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var qrGenerator = serviceProvider.GetService<IQuickResponseCodeGenerator>();
            
            var task = Task.Run(async () => {
                //TODO: final image dimensions
                var qrImage = await qrGenerator.GetQuickResponseCodeAsync("test123", backgroundColor: Colors.Red, foregroundColor: Colors.White, dimensions: 1000);

                File.WriteAllBytes(@"c:\data\qrtest.png", qrImage);
            });

            Console.ReadLine();
        }
    }
}
