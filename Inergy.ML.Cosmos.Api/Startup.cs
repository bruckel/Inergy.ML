using Inergy.ML.Service.Cosmos;
using Inergy.Tools.Architecture.Data.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Imergy.ML.Cosmos.Api
{
    public class Startup
    {
        public IConfiguration configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //* Establecer la configuraci�n de la conexi�n Mongo especificada en settings.json *//
            services.Configure<MongoSettings>(configuration.GetSection(nameof(MongoSettings)));
            services.AddSingleton<IMongoContext>(s => new MongoContext(s.GetRequiredService<IOptions<MongoSettings>>().Value.ConnectionString, s.GetRequiredService<IOptions<MongoSettings>>().Value.DatabaseName));

            //* Establecer la configuraci�n de la conexi�n Mongo especificada en settings.json *//
            services.AddSingleton<ILogger>(l => new LoggerConfiguration().ReadFrom.Configuration(this.configuration.GetSection("SerilogSettings")).CreateLogger());

            //* Inyecci�n de dependencias del servicio *//
            services.AddSingleton<IDataReadingService, DataReadingService>();
            
            services.AddControllers();

            //* Register the Swagger services
            services.AddSwaggerDocument();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();

            app.UseSwaggerUi3();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
