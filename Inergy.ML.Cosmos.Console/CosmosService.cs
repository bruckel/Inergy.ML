using Inergy.ML.Data.Entity;
using Inergy.ML.Service;
using Inergy.Tools.Architecture.Model;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inergy.ML.Cosmos.Application
{
    internal class CosmosService : IHostedService
    {
        private readonly IMLService dataReadingService;
        private readonly ApiContext apiContext;

        public CosmosService(IMLService dataReadingService, ApiContext apiContext)
        {
            this.dataReadingService = dataReadingService;
            this.apiContext = apiContext;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var blogs = this.apiContext.DataReadingMigrationConfigs;

            var first = blogs.First();

            //* Registros de prueba *//
            //Random randomNumber = new Random();

            //IEnumerable<DataReading> dataReadingList = Enumerable.Range(0, 24).Select(i => new DataReading
            //{
            //    Cups = "X12798739123123T",
            //    IdEnergySource = 0,
            //    Type = 0,
            //    TimeStamp = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, i, 0, 0),
            //    Unit = "kWh",
            //    Value = randomNumber.Next(255),
            //    Order = i + 1
            //});

            ////* Insertar registros */
            //this.dataReadingService.CreateDataReadings(dataReadingList, "Europe/Madrid");

            //* Insertar registros */
            //this.dataReadingService.UpdateDataReadings(dataReadingList);

            //* Obtener Reistros *//
            //Console.WriteLine(this.dataReadingService.GetDataReadings("X12798739123123T", dataReadingList.Min(d => d.TimeStamp), dataReadingList.Max(d => d.TimeStamp)));

            //* Eliminar Registros *//
            //this.dataReadingService.DeleteDataReadings("X12798739123123T", dataReadingList.Min(d => d.TimeStamp), dataReadingList.Max(d => d.TimeStamp));

            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
