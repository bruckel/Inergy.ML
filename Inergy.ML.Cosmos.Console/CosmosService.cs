using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Inergy.ML.Model;
using Inergy.ML.Service.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Inergy.ML.Cosmos.Application
{
    internal class CosmosService : IHostedService
    {
        //private readonly IDataReadingService dataReadingService;

        //public CosmosService(IDataReadingService dataReadingService)
        //{
        //    this.dataReadingService = dataReadingService;
        //}


        public CosmosService()
        {
           
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Random randomNumber = new Random();

            //IEnumerable<DataReading> dataReadingList = Enumerable.Range(0, 24).Select(i => new DataReading
            //{
            //    Cups = "X12798739123123T",
            //    IdEnergySource = 0,
            //    Type = 0,
            //    TimeStamp = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, i, 0, 0),
            //    Unit = "kWh",
            //    Value = randomNumber.Next(255)
            //});

            //this.dataReadingService.InsertDataReadings(dataReadingList);

            Console.WriteLine("Hola Mundo");
            
            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {


            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            Console.WriteLine("Hola Mundo");

            //_logger.LogInformation("OnStarted has been called.");

            // Perform post-startup activities here
        }

        private void OnStopping()
        {
            Console.WriteLine("Hola Mundo");

            //_logger.LogInformation("OnStopping has been called.");

            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            Console.WriteLine("Hola Mundo");

            //_logger.LogInformation("OnStopped has been called.");

            // Perform post-stopped activities here
        }
    }
}
