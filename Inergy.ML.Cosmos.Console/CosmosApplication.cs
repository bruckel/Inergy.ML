using Inergy.ML.Service.Cosmos;
using Inergy.Tools.Architecture.Data.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

                //* Establecer la configuración de la conexión Mongo especificada en settings.json *//
                services.Configure<MongoSettings>(hostContext.Configuration.GetSection(nameof(MongoSettings)));
                services.AddSingleton<IMongoContext>(s => new MongoContext(s.GetRequiredService<IOptions<MongoSettings>>().Value.ConnectionString, s.GetRequiredService<IOptions<MongoSettings>>().Value.DatabaseName));
                
                //* Establecer la configuración de la conexión Mongo especificada en settings.json *//
                services.AddSingleton<ILogger>(l => new LoggerConfiguration().ReadFrom.Configuration(hostContext.Configuration.GetSection("SerilogSettings")).CreateLogger());

                //* Inyección de dependencias del servicio *//
                services.AddSingleton<IDataReadingService, DataReadingService>();
            })
            .UseSerilog()
            .Build();
            
            host.StartAsync();
        }
    }
}
