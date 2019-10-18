using Inergy.ML.Service.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;

namespace Inergy.ML.Cosmos.Application
{
    class CosmosApplication
    {
        /// <summary>
        /// Método principal de la aplicación de consola
        /// </summary>
        /// <param name="args">Parámeros</param>
        public static void Main(string[] args)
        {
            var host = new HostBuilder().ConfigureHostConfiguration(configHost =>
            {
                configHost.SetBasePath(Directory.GetCurrentDirectory());
            })
            .ConfigureAppConfiguration((hostContext, configApp) =>
            {
                configApp.AddJsonFile("appsettings.json", false);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<CosmosService>();
                //services.AddSingleton<DataReadingService, DataReadingService>(d => new DataReadingService(hostContext.Configuration.GetSection("MongoConnection:ConnectionString").Value, hostContext.Configuration.GetSection("MongoConnection:Database").Value));
            })
            .ConfigureLogging((hostContext, configLogging) =>
            {
                //configLogging.AddConsole();
                //configLogging.AddDebug();
            })
            //.UseSerilog()
            .Build();
            
            host.StartAsync();
        }
    }
}
