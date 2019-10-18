using System;
using System.Threading;
using System.Threading.Tasks;
using Inergy.ML.Service.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Inergy.ML.Cosmos.Application
{
    internal class CosmosService : IHostedService
    {
        private readonly IDataReadingService dataReadingService;

        public CosmosService(IDataReadingService dataReadingService)
        {
            this.dataReadingService = dataReadingService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Console.WriteLine(dataReadingService.GetHelloWorld());

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {


            return Task.CompletedTask;
        }
    }
}
