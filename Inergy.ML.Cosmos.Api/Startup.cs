using Inergy.ML.Cosmos.Api.Models;
using Inergy.ML.Data;
using Inergy.ML.Service.Cosmos;
using Inergy.Tools.Architecture.Data.Mongo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Text;

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
            //* Context BB.DD. de la autentificación identity *//
            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Api")));
            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();

            //* Establecer la configuración de la conexión Mongo especificada en settings.json *//
            services.Configure<MongoSettings>(configuration.GetSection(nameof(MongoSettings)));
            services.AddSingleton<IMongoContext>(s => new MongoContext(s.GetRequiredService<IOptions<MongoSettings>>().Value.ConnectionString, s.GetRequiredService<IOptions<MongoSettings>>().Value.DatabaseName));

            //* Inyección de dependencias del repositorio *//
            services.AddSingleton<IDataReadingRepository, DataReadingRepository>();

            //* Establecer la configuración de la conexión Mongo especificada en settings.json *//
            services.AddSingleton<ILogger>(l => new LoggerConfiguration().ReadFrom.Configuration(this.configuration).CreateLogger());

            //* Inyección de dependencias del servicio *//
            services.AddSingleton<IDataReadingService, DataReadingService>();

            //* Configurar jwt authentication *//
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("TokenSettings:Key").Value)),
                    ValidIssuer = configuration.GetSection("TokenSettings:Issuer").Value,
                    ValidAudience = configuration.GetSection("TokenSettings:Audience").Value,
                    ValidateLifetime = true
                };
            });

            //* Controladores MVC *//
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

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
