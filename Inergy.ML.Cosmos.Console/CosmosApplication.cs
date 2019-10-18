using System;
using System.IO;
using System.Threading.Tasks;
using Inergy.ML.Service.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inergy.ML.Cosmos.Application
{
    class CosmosApplication
    {
        public static void Main(string[] args)
        {


            var host = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    //configHost.AddJsonFile("hostsettings.json", optional: true);
                    //configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                    //configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddJsonFile("appsettings.json", false);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<CosmosService>();
                    services.AddSingleton<DataReadingService, DataReadingService>(d => new DataReadingService(hostContext.Configuration.GetSection("MongoConnection:ConnectionString").Value, hostContext.Configuration.GetSection("MongoConnection:Database").Value));
                })
                //.ConfigureLogging((hostContext, configLogging) =>
                //{
                //    configLogging.AddConsole();
                //    configLogging.AddDebug();
                //})
                .Build();


            host.StartAsync();

            //var service = new DataReadingService();

            //Console.WriteLine(service.GetHelloWorld());

            //host.con

            //var p = _config.GetValue("");


            //Console.WriteLine("Hola mundo");

            //host.StopAsync();

            //host.Dispose();
        }
    }
}
