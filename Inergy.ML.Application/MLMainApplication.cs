using Inergy.ML.Data;
using Inergy.ML.Data.Entity;
using Inergy.ML.Service;
using Inergy.Tools.Architecture.Data.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System.IO;
using Microsoft.ML;

namespace Inergy.ML.Application
{
    class MLMainApplication
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
                services.AddDbContext<ApiContext>(options => options.UseSqlServer(hostContext.Configuration.GetConnectionString("Api")));

                //* Establecer la configuración de la conexión Mongo especificada en settings.json *//
                services.Configure<MongoSettings>(hostContext.Configuration.GetSection(nameof(MongoSettings)));
                services.AddSingleton<IMongoContext>(s => new MongoContext(s.GetRequiredService<IOptions<MongoSettings>>().Value.ConnectionString, s.GetRequiredService<IOptions<MongoSettings>>().Value.DatabaseName));

                //* Inyección de dependencias del repositorio *//
                services.AddSingleton<IDataReadingRepository, DataReadingRepository>();

                //* Establecer la configuración de la conexión Mongo especificada en settings.json *//
                services.AddSingleton<ILogger>(l => new LoggerConfiguration().ReadFrom.Configuration(hostContext.Configuration.GetSection("SerilogSettings")).CreateLogger());

                //* Contexto de ML .Net *//
                services.AddSingleton<MLContext>(m => new MLContext());

                //* Inyección de dependencias del servicio *//
                services.AddSingleton<IDataReadingService, DataReadingService>();
                services.AddSingleton<IMLService, MLService>();

                services.AddHostedService<MLMainService>();
            })
            .UseSerilog()
            .Build();

            host.StartAsync();
        }
    }
}
