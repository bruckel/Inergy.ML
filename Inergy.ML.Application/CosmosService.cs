using Inergy.ML.Data.Entity;
using Inergy.ML.Service;
using Inergy.Tools.Architecture.Model;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inergy.ML.Application
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

        /// <summary>
        /// Inicio de la aplicación de consola
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            

            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
