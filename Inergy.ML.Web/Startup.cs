using Inergy.ML.Data;
using Inergy.ML.Data.Entity;
using Inergy.ML.Service;
using Inergy.ML.Web.Data;
using Inergy.Tools.Architecture.Data.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Serilog;

namespace Inergy.ML.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApiContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Api")), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

            //* Establecer la configuración de la conexión Mongo especificada en settings.json *//
            services.Configure<MongoSettings>(Configuration.GetSection(nameof(MongoSettings)));
            services.AddSingleton<IMongoContext>(s => new MongoContext(s.GetRequiredService<IOptions<MongoSettings>>().Value.ConnectionString, s.GetRequiredService<IOptions<MongoSettings>>().Value.DatabaseName));

            //* Inyección de dependencias del repositorio *//
            services.AddSingleton<IDataReadingRepository, DataReadingRepository>();

            //* Establecer la configuración de la conexión Mongo especificada en settings.json *//
            services.AddSingleton<ILogger>(l => new LoggerConfiguration().ReadFrom.Configuration(this.Configuration).CreateLogger());

            services.AddSingleton<MLContext>(m => new MLContext());

            services.AddSingleton<IMLService, MLService>();
            services.AddSingleton<IAnomalyDetectionService, AnomalyDetectionService>();
            services.AddSingleton<IApiService, ApiService>();

            //services.AddSingleton<WeatherForecastService>();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddTelerikBlazor();
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
